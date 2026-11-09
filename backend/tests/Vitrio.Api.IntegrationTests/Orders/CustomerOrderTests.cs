using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Search;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class CustomerOrderTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task CreateOrder_ForTomorrowAfternoon_IsSentAndDueAtTheEndOfThePeriod()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var tomorrow = factory.Tomorrow();

        var order = await _client.CreateOrderOkAsync(scenario, tomorrow);

        Assert.Equal(OrderStatus.Sent, order.Status);
        Assert.Equal(LocalTime(tomorrow, 18), order.RespondBy);
        Assert.Equal("Outro", order.ServiceTitle);
        Assert.Equal(1, order.CategoryId);
        Assert.Equal("Bairro de Teste", order.Neighborhood);
        Assert.Equal("Carlos Menezes", order.Provider.DisplayName);
        Assert.Equal("+5516991234567", order.Provider.WhatsAppPhone);
        Assert.Equal([OrderAction.Cancel], order.AllowedActions);
        Assert.Null(order.Quote);
    }

    [Fact]
    public async Task CreateOrder_AnswersTheTimesInTheTimeZoneOfTheOrderCity()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var tomorrow = factory.Tomorrow();

        var order = await _client.CreateOrderOkAsync(scenario, tomorrow);

        var cityOffset = TimeSpan.FromHours(-3);
        Assert.Equal((cityOffset, cityOffset), (order.CreatedAt.Offset, order.RespondBy.Offset));
        Assert.Equal((18, 0), (order.RespondBy.Hour, order.RespondBy.Minute));
    }

    [Fact]
    public async Task CreateOrder_ForAServiceOfTheProvider_UsesTheServiceTitle()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var profile = await (await _client.GetAsync($"/api/v1/providers/{scenario.Provider.Id}", TestContext.Current.CancellationToken))
            .Content.ReadApiAsync<Application.Providers.Common.ProviderPublicResponse>();
        var service = Assert.Single(profile.Services);

        var response = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow(), serviceId: service.Id));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("Troca de tomada", (await response.Content.ReadApiAsync<OrderCustomerView>()).ServiceTitle);
    }

    [Fact]
    public async Task CreateOrder_RepeatedWithTheSameIdempotencyKey_CreatesOnlyOneOrder()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var body = NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow());
        var key = Guid.NewGuid();

        var first = await _client.CreateOrderAsync(scenario.Customer.AccessToken, body, key);
        var second = await _client.CreateOrderAsync(scenario.Customer.AccessToken, body, key);

        Assert.Equal(HttpStatusCode.Created, first.StatusCode);
        Assert.Equal(HttpStatusCode.Created, second.StatusCode);
        Assert.Equal((await first.Content.ReadApiAsync<OrderCustomerView>()).Id, (await second.Content.ReadApiAsync<OrderCustomerView>()).Id);
        Assert.Equal(1, await CountOrdersAsync(scenario.Customer.Id));
    }

    [Fact]
    public async Task CreateOrder_ReusingTheKeyWithAnotherBody_ReturnsKeyReused()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var key = Guid.NewGuid();
        await _client.CreateOrderAsync(scenario.Customer.AccessToken, NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow()), key);

        var response = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow(), title: "Tomada sem energia na sala"),
            key);

        await response.AssertProblemAsync(HttpStatusCode.UnprocessableEntity, "idempotency.key_reused");
        Assert.Equal(1, await CountOrdersAsync(scenario.Customer.Id));
    }

    [Fact]
    public async Task CreateOrder_WithoutIdempotencyKey_ReturnsBadRequest()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);

        var response = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow()),
            withKey: false);

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
    }

    [Fact]
    public async Task CreateOrder_ForTheCustomersOwnShowcase_ReturnsConflict()
    {
        var spot = SearchApi.NextSpot();
        var provider = await factory.PublishAsync(_client, new SearchableProvider(spot));
        var me = await (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", provider.AccessToken)).Content.ReadApiAsync<Application.Accounts.Common.MeResponse>();
        var owner = await _client.PrepareCustomerAsync(factory, provider.AccessToken, me.Id, spot);

        var response = await _client.CreateOrderAsync(owner.AccessToken, NewOrder(provider.Id, owner.AddressId, factory.Tomorrow()));

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.own_provider");
    }

    [Fact]
    public async Task CreateOrder_WithoutAPhoneOnTheAccount_ReturnsPhoneRequiredAndCreatesNothing()
    {
        var spot = SearchApi.NextSpot();
        var provider = await factory.PublishAsync(_client, new SearchableProvider(spot));
        var customer = await factory.SignInCustomerAsync(_client, spot.Offset(1), withPhone: false);

        var response = await _client.CreateOrderAsync(customer.AccessToken, NewOrder(provider.Id, customer.AddressId, factory.Tomorrow()));

        await response.AssertProblemAsync(HttpStatusCode.UnprocessableEntity, "order.phone_required");
        Assert.Equal(0, await CountOrdersAsync(customer.Id));
    }

    [Fact]
    public async Task CreateOrder_ThirtyOneDaysAhead_ReturnsBadRequest()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);

        var response = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow().AddDays(30)));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "order.invalid_preferred_day");
    }

    [Fact]
    public async Task CreateOrder_ForAPausedShowcase_ReturnsNotReceivingOrders()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var paused = await _client.SendAsAsync(HttpMethod.Post, "/api/v1/provider/pause", scenario.Provider.AccessToken);
        Assert.True(paused.IsSuccessStatusCode, $"{(int)paused.StatusCode}");

        var response = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow()));

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.provider_not_receiving");
    }

    [Fact]
    public async Task CreateOrder_ForAnUnknownShowcaseOrAnotherPersonsAddress_ReturnsNotFound()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var stranger = await factory.SignInCustomerAsync(_client, scenario.Spot);

        var unknownProvider = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(Guid.NewGuid(), scenario.Customer.AddressId, factory.Tomorrow()));
        var foreignAddress = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, stranger.AddressId, factory.Tomorrow()));

        await unknownProvider.AssertProblemAsync(HttpStatusCode.NotFound, "provider.not_found");
        await foreignAddress.AssertProblemAsync(HttpStatusCode.NotFound, "address.not_found");
    }

    [Fact]
    public async Task CreateOrder_FourthWaitingInTheSameCategory_IsRefusedButAnotherCategoryIsAccepted()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        for (var index = 0; index < 3; index++)
        {
            await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());
        }

        var plumber = await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(2)) { CategoryIds = [9] });

        var sameCategory = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow()));
        var otherCategory = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(plumber.Id, scenario.Customer.AddressId, factory.Tomorrow()));

        await sameCategory.AssertProblemAsync(HttpStatusCode.Conflict, "order.too_many_waiting");
        Assert.Equal(HttpStatusCode.Created, otherCategory.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_WithAnUploadOfAnotherPerson_ReturnsUploadNotFound()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);

        var response = await _client.CreateOrderAsync(
            scenario.Customer.AccessToken,
            NewOrder(scenario.Provider.Id, scenario.Customer.AddressId, factory.Tomorrow(), [$"orders/{Guid.NewGuid():N}/foto"]));

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "order.photo_upload_not_found");
    }

    [Fact]
    public async Task ListOrders_SplitsOpenAndClosedOrders_WithTheNewestFirst()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var scheduled = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed();
                order.AcceptQuote(1, now).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromMinutes(30));
        var cancelled = await factory.SeedOrderAsync(
            scenario,
            order => order.Cancel(OrderParty.Customer, CancellationReason.ResolvedAnotherWay, null, now).ShouldSucceed(),
            createdAgo: TimeSpan.FromMinutes(20));
        var sent = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow());

        var open = await ListAsync(scenario.Customer.AccessToken, "open");
        var closed = await ListAsync(scenario.Customer.AccessToken, "closed");

        Assert.Equal([sent.Id, scheduled], open.Items.Select(item => item.Id));
        Assert.Equal(OrderStatus.Scheduled, open.Items[1].Status);
        Assert.Equal("14:00", open.Items[1].Schedule!.Start);
        Assert.Equal("Carlos Menezes", open.Items[0].ProviderName);
        Assert.Equal(cancelled, Assert.Single(closed.Items).Id);
        Assert.False(open.HasMore);
    }

    [Fact]
    public async Task OrderOfAnotherCustomer_IsNotFoundOnEveryRoute()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var orderId = await factory.SeedOrderAsync(scenario, order => order.Decline(DeclineReason.NoAvailability, null, factory.ClockAgo(TimeSpan.Zero)).ShouldSucceed());
        var stranger = await factory.SignInCustomerAsync(_client, scenario.Spot);

        var responses = new[]
        {
            await _client.SendAsAsync(HttpMethod.Get, $"{Route}/{orderId}", stranger.AccessToken),
            await _client.SendAsAsync(HttpMethod.Get, $"{Route}/{orderId}/suggestions", stranger.AccessToken),
            await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{orderId}/photos/copies", stranger.AccessToken),
            await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/quote/accept", stranger.AccessToken, new { quoteVersion = 1 }),
            await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{orderId}/quote/decline", stranger.AccessToken),
            await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/cancel", stranger.AccessToken, new { reason = "resolvedAnotherWay" }),
            await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{orderId}/complete", stranger.AccessToken)
        };

        foreach (var response in responses)
        {
            await response.AssertProblemAsync(HttpStatusCode.NotFound, "order.not_found");
        }
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("POST", "")]
    [InlineData("POST", "/photos/upload-url")]
    [InlineData("GET", "/{id}")]
    [InlineData("GET", "/{id}/suggestions")]
    [InlineData("POST", "/{id}/photos/copies")]
    [InlineData("POST", "/{id}/quote/accept")]
    [InlineData("POST", "/{id}/quote/decline")]
    [InlineData("POST", "/{id}/cancel")]
    [InlineData("POST", "/{id}/complete")]
    public async Task OrderRoutes_WithoutToken_ReturnUnauthorized(string method, string path)
    {
        var url = Route + path.Replace("{id}", Guid.NewGuid().ToString(), StringComparison.Ordinal);

        var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), url), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task OrderDetail_WithTheWhatsAppButtonOff_DoesNotShowTheProvidersPhone()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var turnedOff = await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/provider", scenario.Provider.AccessToken, new { whatsAppEnabled = false });
        Assert.True(turnedOff.IsSuccessStatusCode, $"{(int)turnedOff.StatusCode}");
        var orderId = await factory.SeedOrderAsync(scenario);

        var order = await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId);

        Assert.Null(order.Provider.WhatsAppPhone);
    }

    private async Task<OrderCustomerPage> ListAsync(string accessToken, string status) =>
        await (await _client.SendAsAsync(HttpMethod.Get, $"{Route}?status={status}", accessToken)).Content.ReadApiAsync<OrderCustomerPage>();

    private Task<int> CountOrdersAsync(Guid customerId) =>
        factory.QueryAsync(db => db.Orders.CountAsync(order => order.CustomerId == customerId, TestContext.Current.CancellationToken));
}
