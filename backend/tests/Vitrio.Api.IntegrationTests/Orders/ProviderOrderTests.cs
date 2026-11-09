using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class ProviderOrderTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task NewOrder_ShowsInTheNewTabWithNeighborhoodDistanceAndShortName()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());

        var page = await (await _client.SendAsAsync(HttpMethod.Get, $"{ProviderRoute}?tab=new", scenario.Provider.AccessToken))
            .Content.ReadApiAsync<OrderProviderPage>();

        var item = Assert.Single(page.Items);
        Assert.Equal((created.Id, Title, "Bairro de Teste", "Bruno C."), (item.Id, item.Title, item.Neighborhood, item.CustomerName));
        Assert.NotNull(item.DistanceKm);
        Assert.InRange(item.DistanceKm!.Value, 0, 3);
    }

    [Fact]
    public async Task SentOrderDetail_HidesTheAddressPhoneAndSurname()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());

        var response = await _client.SendAsAsync(HttpMethod.Get, $"{ProviderRoute}/{created.Id}", scenario.Provider.AccessToken);
        var raw = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("Rua de Teste", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("Costa", raw, StringComparison.Ordinal);
        Assert.DoesNotContain("988887777", raw, StringComparison.Ordinal);
        var order = await response.Content.ReadApiAsync<OrderProviderView>();
        Assert.Null(order.Address);
        Assert.Equal(("Bruno C.", null, null), (order.Customer.DisplayName, order.Customer.FullName, order.Customer.Phone));
        Assert.Equal([OrderAction.SendQuote, OrderAction.Decline], order.AllowedActions);
    }

    [Fact]
    public async Task SendQuote_ForThreeDaysAheadAtTwoForTwoHours_QuotesTheOrderForTheCustomer()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        var date = factory.Tomorrow().AddDays(2);

        var response = await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, QuoteBody(date, durationMinutes: 120));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sent = await response.Content.ReadApiAsync<OrderProviderView>();
        Assert.Equal(OrderStatus.Quoted, sent.Status);
        Assert.Equal(sent.Quote!.SentAt + TimeSpan.FromHours(48), sent.Quote.ValidUntil);
        var customerView = await _client.GetOrderAsync(scenario.Customer.AccessToken, created.Id);
        Assert.Equal((180m, date, "14:00", "16:00", "disjuntor 40 A"), (customerView.Quote!.Amount, customerView.Quote.Time.Date, customerView.Quote.Time.Start, customerView.Quote.Time.End, customerView.Quote.MaterialDetails));
        Assert.NotNull(await FirstResponseAtAsync(created.Id));
    }

    [Fact]
    public async Task SendQuote_ReplacingIt_RaisesTheVersionAndKeepsTheFirstResponse()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        var date = factory.Tomorrow().AddDays(2);
        await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, QuoteBody(date));
        var firstResponse = await FirstResponseAtAsync(created.Id);

        var replaced = await (await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, QuoteBody(date, start: "16:00", amount: 200m)))
            .Content.ReadApiAsync<OrderProviderView>();

        Assert.Equal((2, 200m, "16:00"), (replaced.Quote!.Version, replaced.Quote.Amount, replaced.Quote.Time.Start));
        Assert.Equal(firstResponse, await FirstResponseAtAsync(created.Id));
        Assert.Equal([OrderAction.ReplaceQuote], replaced.AllowedActions);
    }

    [Fact]
    public async Task SendQuote_RepeatedWithTheSameKey_SendsItOnce()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        var body = QuoteBody(factory.Tomorrow().AddDays(2));
        var key = Guid.NewGuid();

        var first = await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, body, key);
        var second = await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, body, key);

        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(1, (await first.Content.ReadApiAsync<OrderProviderView>()).Quote!.Version);
        Assert.Equal(1, (await second.Content.ReadApiAsync<OrderProviderView>()).Quote!.Version);
        Assert.Equal(1, (await _client.GetOrderAsync(scenario.Customer.AccessToken, created.Id)).Quote!.Version);
    }

    [Fact]
    public async Task SendQuote_WithMaterialDetailsButNoMaterial_ReturnsBadRequest()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());

        var response = await _client.SendQuoteAsync(
            scenario.Provider.AccessToken,
            created.Id,
            QuoteBody(factory.Tomorrow().AddDays(2), materialIncluded: false));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "quote.invalid_material_details");
    }

    [Fact]
    public async Task SendQuote_AfterTheCustomerCancelled_ReturnsConflict()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{created.Id}/cancel", scenario.Customer.AccessToken, new { reason = "resolvedAnotherWay" });

        var response = await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, QuoteBody(factory.Tomorrow().AddDays(2)));

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.cannot_quote");
    }

    [Fact]
    public async Task DeclineOrder_RequiresAReasonFiltersContactsAndShowsTheReasonToTheCustomer()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        var decline = $"{ProviderRoute}/{created.Id}/decline";

        var withoutReason = await _client.SendJsonAsAsync(HttpMethod.Post, decline, scenario.Provider.AccessToken, new { });
        var withPhone = await _client.SendJsonAsAsync(HttpMethod.Post, decline, scenario.Provider.AccessToken, new { reason = "other", details = "Me liga no 16 99123-4567" });
        var declined = await _client.SendJsonAsAsync(HttpMethod.Post, decline, scenario.Provider.AccessToken, new { reason = "outsideArea" });

        await withoutReason.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        await withPhone.AssertProblemAsync(HttpStatusCode.BadRequest, "order.contact_in_details");
        Assert.Equal(HttpStatusCode.OK, declined.StatusCode);
        var customerView = await _client.GetOrderAsync(scenario.Customer.AccessToken, created.Id);
        Assert.Equal((OrderStatus.Declined, DeclineReason.OutsideArea), (customerView.Status, customerView.Decline!.Reason));
    }

    [Fact]
    public async Task AcceptedOrder_ShowsTheAddressAndContactToTheProviderAndAuditsTheRead()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        await _client.SendQuoteAsync(scenario.Provider.AccessToken, created.Id, QuoteBody(factory.Tomorrow().AddDays(2)));
        await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{created.Id}/quote/accept", scenario.Customer.AccessToken, new { quoteVersion = 1 });

        var order = await _client.GetProviderOrderAsync(scenario.Provider.AccessToken, created.Id);

        Assert.Equal(OrderStatus.Scheduled, order.Status);
        Assert.Equal(("Rua de Teste", "45", "Bruno Costa", "+5516988887777"), (order.Address!.Street, order.Address.Number, order.Customer.FullName, order.Customer.Phone));
        Assert.Equal(LocalTime(factory.Tomorrow().AddDays(2), 15) + TimeSpan.FromDays(7), order.ContactVisibleUntil);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            audit => audit.Action == OrderAudit.ContactRead && audit.TargetId == created.Id,
            TestContext.Current.CancellationToken)));
        var page = await (await _client.SendAsAsync(HttpMethod.Get, $"{ProviderRoute}?tab=ongoing", scenario.Provider.AccessToken))
            .Content.ReadApiAsync<OrderProviderPage>();
        Assert.Equal("Bruno Costa", Assert.Single(page.Items).CustomerName);
    }

    [Fact]
    public async Task ScheduledOrderThatEndedEightDaysAgo_NoLongerShowsTheAddress()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var tenDaysAgo = factory.ClockAgo(TimeSpan.FromDays(10));
        var orderId = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(tenDaysAgo).AddDays(2), startHour: 10), tenDaysAgo).ShouldSucceed();
                order.AcceptQuote(1, tenDaysAgo).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromDays(10));

        var order = await _client.GetProviderOrderAsync(scenario.Provider.AccessToken, orderId);

        Assert.Null(order.Address);
        Assert.Null(order.Customer.Phone);
        Assert.Equal("Bruno C.", order.Customer.DisplayName);
    }

    [Fact]
    public async Task PaymentConfirmedOrderThatEndedThreeDaysAgo_StillShowsTheAddress()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var fiveDaysAgo = factory.ClockAgo(TimeSpan.FromDays(5));
        var orderId = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(fiveDaysAgo).AddDays(2), startHour: 10), fiveDaysAgo).ShouldSucceed();
                order.AcceptQuote(1, fiveDaysAgo).ShouldSucceed();
                order.Complete(OrderParty.Provider, factory.ClockAgo(TimeSpan.FromDays(2))).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromDays(5));
        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"update orders set status = 'PaymentConfirmed' where id = {orderId}",
            TestContext.Current.CancellationToken));

        var order = await _client.GetProviderOrderAsync(scenario.Provider.AccessToken, orderId);

        Assert.Equal(OrderStatus.PaymentConfirmed, order.Status);
        Assert.Equal("Rua de Teste", order.Address!.Street);
    }

    [Fact]
    public async Task Reschedule_MovesTheServiceKeepsItScheduledAndPublishesTheEvent()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(2)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
        });
        var newDay = LocalToday(now).AddDays(3);

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Post,
            $"{ProviderRoute}/{orderId}/reschedule",
            scenario.Provider.AccessToken,
            new { date = newDay, start = "09:00", durationMinutes = 60 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadApiAsync<OrderProviderView>();
        Assert.Equal((OrderStatus.Scheduled, 1, newDay, "09:00"), (order.Status, order.RescheduleCount, order.Schedule!.Date, order.Schedule.Start));
        Assert.Equal(LocalTime(newDay, 10) + TimeSpan.FromDays(7), order.ContactVisibleUntil);
        var rescheduled = await factory.QueryAsync(db => db.OutboxMessages
            .Where(message => message.Type == "order.rescheduled")
            .Select(message => message.Payload)
            .ToListAsync(TestContext.Current.CancellationToken));
        Assert.Contains(rescheduled, payload => payload.Contains(orderId.ToString(), StringComparison.Ordinal));
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            audit => audit.Action == OrderAudit.Rescheduled && audit.TargetId == orderId,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task Reschedule_AThirdTimeOrAfterTheStart_ReturnsConflict()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var twice = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(2)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
            order.Reschedule(LocalToday(now).AddDays(3), new TimeOnly(9, 0), 60, now).ShouldSucceed();
            order.Reschedule(LocalToday(now).AddDays(4), new TimeOnly(9, 0), 60, now).ShouldSucceed();
        });
        var threeDaysAgo = factory.ClockAgo(TimeSpan.FromDays(3));
        var started = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(1), startHour: 10), threeDaysAgo).ShouldSucceed();
                order.AcceptQuote(1, threeDaysAgo).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromDays(3));
        var body = new { date = LocalToday(now).AddDays(5), start = "09:00", durationMinutes = 60 };

        var third = await _client.SendJsonAsAsync(HttpMethod.Post, $"{ProviderRoute}/{twice}/reschedule", scenario.Provider.AccessToken, body);
        var late = await _client.SendJsonAsAsync(HttpMethod.Post, $"{ProviderRoute}/{started}/reschedule", scenario.Provider.AccessToken, body);

        await third.AssertProblemAsync(HttpStatusCode.Conflict, "order.reschedule_limit");
        await late.AssertProblemAsync(HttpStatusCode.Conflict, "order.cannot_reschedule");
    }

    [Fact]
    public async Task ProviderCancellation_InsideTwoHours_IsAuditedAndTheCustomerSeesOnlyTheReason()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var threeHoursAgo = factory.ClockAgo(TimeSpan.FromHours(3));
        var startsSoon = factory.ClockAgo(TimeSpan.FromMinutes(-90)).UtcNow;
        var local = CityTimeOf(startsSoon);
        var orderId = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(new QuoteTerms(QuoteType.FixedPrice, 120m, local.Date, local.Start, 30, false, null, null), threeHoursAgo).ShouldSucceed();
                order.AcceptQuote(1, threeHoursAgo).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromHours(3));

        var response = await _client.SendJsonAsAsync(
            HttpMethod.Post,
            $"{ProviderRoute}/{orderId}/cancel",
            scenario.Provider.AccessToken,
            new { reason = "unforeseen", details = "Carro quebrou no caminho" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            audit => audit.Action == OrderAudit.LateCancellation && audit.TargetId == orderId,
            TestContext.Current.CancellationToken)));
        var customerView = await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId);
        Assert.Equal((OrderParty.Provider, CancellationReason.Unforeseen, null), (customerView.Cancellation!.By, customerView.Cancellation.Reason, customerView.Cancellation.Details));
    }

    [Fact]
    public async Task CustomerCancellation_ShowsTheProviderOnlyTheReason()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{created.Id}/cancel", scenario.Customer.AccessToken, new { reason = "other", details = "Mudei de casa" });

        var order = await _client.GetProviderOrderAsync(scenario.Provider.AccessToken, created.Id);

        Assert.Equal((OrderParty.Customer, CancellationReason.Other, null), (order.Cancellation!.By, order.Cancellation.Reason, order.Cancellation.Details));
    }

    [Fact]
    public async Task CompleteByProvider_BeforeTheStart_ReturnsConflict()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(2)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
        });

        var response = await _client.SendAsAsync(HttpMethod.Post, $"{ProviderRoute}/{orderId}/complete", scenario.Provider.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.complete_before_start");
    }

    [Fact]
    public async Task OrderOfAnotherProvider_IsNotFoundOnEveryRoute()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var orderId = await factory.SeedOrderAsync(scenario);
        var other = await factory.StartOrderScenarioAsync(_client);
        var token = other.Provider.AccessToken;
        var future = factory.Tomorrow().AddDays(2);

        var responses = new[]
        {
            await _client.SendAsAsync(HttpMethod.Get, $"{ProviderRoute}/{orderId}", token),
            await _client.SendQuoteAsync(token, orderId, QuoteBody(future)),
            await _client.SendJsonAsAsync(HttpMethod.Post, $"{ProviderRoute}/{orderId}/decline", token, new { reason = "noAvailability" }),
            await _client.SendJsonAsAsync(HttpMethod.Post, $"{ProviderRoute}/{orderId}/reschedule", token, new { date = future, start = "09:00", durationMinutes = 60 }),
            await _client.SendJsonAsAsync(HttpMethod.Post, $"{ProviderRoute}/{orderId}/cancel", token, new { reason = "unforeseen" }),
            await _client.SendAsAsync(HttpMethod.Post, $"{ProviderRoute}/{orderId}/complete", token)
        };

        foreach (var response in responses)
        {
            await response.AssertProblemAsync(HttpStatusCode.NotFound, "order.not_found");
        }
    }

    [Theory]
    [InlineData("GET", "/orders")]
    [InlineData("GET", "/orders/{id}")]
    [InlineData("POST", "/orders/{id}/quote")]
    [InlineData("POST", "/orders/{id}/decline")]
    [InlineData("POST", "/orders/{id}/reschedule")]
    [InlineData("POST", "/orders/{id}/cancel")]
    [InlineData("POST", "/orders/{id}/complete")]
    [InlineData("GET", "/agenda")]
    public async Task ProviderOrderRoutes_RefuseAnonymousAndCustomerOnlyAccounts(string method, string path)
    {
        var url = "/api/v1/provider" + path.Replace("{id}", Guid.NewGuid().ToString(), StringComparison.Ordinal);
        var customer = await factory.SignInNewAccountAsync(_client);

        var anonymous = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url), TestContext.Current.CancellationToken);
        var notProvider = await _client.SendAsAsync(new HttpMethod(method), url, customer.AccessToken);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, notProvider.StatusCode);
    }

    private Task<DateTimeOffset?> FirstResponseAtAsync(Guid orderId) =>
        factory.QueryAsync(db => db.Orders.Where(order => order.Id == orderId).Select(order => order.FirstResponseAt)
            .SingleAsync(TestContext.Current.CancellationToken));
}
