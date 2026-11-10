using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Search;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Orders;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class CustomerOrderActionTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();
    private readonly HttpClient _storage = new();

    [Fact]
    public async Task QuotedOrder_ListsAcceptDeclineAndCancelForTheCustomer()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order => order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed());

        var order = await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId);

        Assert.Equal(OrderStatus.Quoted, order.Status);
        Assert.Equal([OrderAction.AcceptQuote, OrderAction.DeclineQuote, OrderAction.Cancel], order.AllowedActions);
        Assert.Equal(180m, order.Quote!.Amount);
        Assert.Equal(("14:00", "15:00"), (order.Quote.Time.Start, order.Quote.Time.End));
        Assert.Equal("disjuntor 40 A", order.Quote.MaterialDetails);
        Assert.Equal(1, order.Quote.Version);
    }

    [Fact]
    public async Task AcceptQuote_WithAnOldVersion_ReturnsConflict()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed();
            order.SendQuote(Terms(LocalToday(now).AddDays(3), startHour: 16), now).ShouldSucceed();
        });

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/quote/accept", scenario.Customer.AccessToken, new { quoteVersion = 1 });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "quote.version_mismatch");
    }

    [Fact]
    public async Task AcceptQuote_WithinTheValidity_SchedulesTheServiceAndAuditsTheAddressRelease()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order => order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed());

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/quote/accept", scenario.Customer.AccessToken, new { quoteVersion = 1 });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadApiAsync<OrderCustomerView>();
        Assert.Equal(OrderStatus.Scheduled, order.Status);
        Assert.Equal(LocalToday(now).AddDays(3), order.Schedule!.Date);
        Assert.Equal([OrderAction.Cancel], order.AllowedActions);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            audit => audit.Action == OrderAudit.AddressReleased && audit.TargetId == orderId && audit.ActorId == scenario.Customer.Id,
            TestContext.Current.CancellationToken)));
    }

    [Fact]
    public async Task AcceptQuote_AfterTheValidity_ReturnsQuoteExpired()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var threeDaysAgo = factory.ClockAgo(TimeSpan.FromDays(3));
        var orderId = await factory.SeedOrderAsync(
            scenario,
            order => order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(5)), threeDaysAgo).ShouldSucceed(),
            createdAgo: TimeSpan.FromDays(3));

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/quote/accept", scenario.Customer.AccessToken, new { quoteVersion = 1 });

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "quote.expired");
        Assert.Equal([OrderAction.Cancel], (await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId)).AllowedActions);
    }

    [Fact]
    public async Task DeclineQuote_ClosesTheOrder()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order => order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed());

        var response = await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{orderId}/quote/decline", scenario.Customer.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadApiAsync<OrderCustomerView>();
        Assert.Equal(OrderStatus.QuoteDeclined, order.Status);
        Assert.Empty(order.AllowedActions);
    }

    [Fact]
    public async Task CancelScheduledOrder_WithoutReason_IsRefusedAndWithReason_IsCancelled()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var orderId = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
        });

        var withoutReason = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/cancel", scenario.Customer.AccessToken, new { });
        var withReason = await _client.SendJsonAsAsync(
            HttpMethod.Post,
            $"{Route}/{orderId}/cancel",
            scenario.Customer.AccessToken,
            new { reason = "other", details = "Mudei de casa" });

        await withoutReason.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.Equal(HttpStatusCode.OK, withReason.StatusCode);
        var order = await withReason.Content.ReadApiAsync<OrderCustomerView>();
        Assert.Equal(OrderStatus.Cancelled, order.Status);
        Assert.Equal((OrderParty.Customer, CancellationReason.Other, "Mudei de casa"), (order.Cancellation!.By, order.Cancellation.Reason, order.Cancellation.Details));
    }

    [Fact]
    public async Task CancelOrder_WithAProviderReason_ReturnsBadRequest()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var orderId = await factory.SeedOrderAsync(scenario);

        var response = await _client.SendJsonAsAsync(HttpMethod.Post, $"{Route}/{orderId}/cancel", scenario.Customer.AccessToken, new { reason = "unforeseen" });

        await response.AssertProblemAsync(HttpStatusCode.BadRequest, "order.invalid_cancellation_reason");
    }

    [Fact]
    public async Task CompleteOrder_BeforeTheStart_IsRefusedAndAfterTheStart_IsCompleted()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var future = await factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
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

        var tooEarly = await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{future}/complete", scenario.Customer.AccessToken);
        var done = await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{started}/complete", scenario.Customer.AccessToken);

        await tooEarly.AssertProblemAsync(HttpStatusCode.Conflict, "order.complete_before_start");
        Assert.Equal(HttpStatusCode.OK, done.StatusCode);
        var order = await done.Content.ReadApiAsync<OrderCustomerView>();
        Assert.Equal((OrderStatus.Completed, OrderParty.Customer), (order.Status, order.CompletedBy));
    }

    [Fact]
    public async Task Suggestions_ForADeclinedOrder_ListNearbyProvidersOfTheCategoryWithoutTheOneWhoDeclined()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var first = await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(2)) { DisplayName = "Rogério Alves" });
        var second = await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(-2)) { DisplayName = "Sandra Lopes" });
        await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(1, 1)) { CategoryIds = [9] });
        var now = factory.ClockAgo(TimeSpan.Zero);
        var declined = await factory.SeedOrderAsync(scenario, order => order.Decline(DeclineReason.OutsideArea, null, now).ShouldSucceed());
        var quoted = await factory.SeedOrderAsync(scenario, order => order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed());

        var suggestions = await (await _client.SendAsAsync(HttpMethod.Get, $"{Route}/{declined}/suggestions", scenario.Customer.AccessToken))
            .Content.ReadApiAsync<List<ProviderSearchItem>>();
        var notYet = await _client.SendAsAsync(HttpMethod.Get, $"{Route}/{quoted}/suggestions", scenario.Customer.AccessToken);

        Assert.Equal(new[] { first.Id, second.Id }.Order(), suggestions.Select(item => item.Id).Order());
        await notYet.AssertProblemAsync(HttpStatusCode.Conflict, "order.no_suggestions");
    }

    [Fact]
    public async Task Suggestions_ForACustomerWhoIsAlsoAProvider_LeaveOutTheirOwnShowcase()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var other = await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(2)) { DisplayName = "Rogério Alves" });
        var own = await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(-2)) { DisplayName = "Mariana Souza" });
        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"update providers set user_id = {scenario.Customer.Id} where id = {own.Id}",
            TestContext.Current.CancellationToken));
        var declined = await factory.SeedOrderAsync(
            scenario,
            order => order.Decline(DeclineReason.OutsideArea, null, factory.ClockAgo(TimeSpan.Zero)).ShouldSucceed());

        var suggestions = await (await _client.SendAsAsync(HttpMethod.Get, $"{Route}/{declined}/suggestions", scenario.Customer.AccessToken))
            .Content.ReadApiAsync<List<ProviderSearchItem>>();

        Assert.Equal([other.Id], suggestions.Select(item => item.Id));
    }

    [Fact]
    public async Task DeclinedOrder_ShowsTheReasonToTheCustomer()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var orderId = await factory.SeedOrderAsync(scenario, order => order.Decline(DeclineReason.OutsideArea, null, factory.ClockAgo(TimeSpan.Zero)).ShouldSucceed());

        var order = await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId);

        Assert.Equal(OrderStatus.Declined, order.Status);
        Assert.Equal(DeclineReason.OutsideArea, order.Decline!.Reason);
    }

    [Fact]
    public async Task CopyPhotos_OfADeclinedOrder_GivesNewKeysThatCreateANewOrderWithThePhotos()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var orderId = await SeedDeclinedOrderWithStoredPhotosAsync(scenario, 2);

        var copies = await (await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{orderId}/photos/copies", scenario.Customer.AccessToken))
            .Content.ReadApiAsync<List<OrderPhotoCopyResponse>>();
        var preview = await _storage.GetAsync(copies[0].PreviewUrl, TestContext.Current.CancellationToken);
        var created = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow(), [.. copies.Select(copy => copy.ObjectKey)]);

        Assert.Equal(2, copies.Count);
        Assert.Equal(HttpStatusCode.OK, preview.StatusCode);
        Assert.Equal(2, created.Photos.Count);
        Assert.All(created.Photos, photo => Assert.Equal(OrderPhotoStatus.Processing, photo.Status));
        var original = await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId);
        Assert.Equal(2, original.Photos.Count(photo => photo is { Status: OrderPhotoStatus.Ready, Url: not null }));
    }

    [Fact]
    public async Task CopyPhotos_AfterTheyWereErased_ReturnsPhotosRemoved()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var longAgo = factory.ClockAgo(TimeSpan.FromDays(100));
        var orderId = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.Decline(DeclineReason.NoAvailability, null, longAgo).ShouldSucceed();
                order.ErasePersonalData(factory.ClockAgo(TimeSpan.Zero));
            },
            createdAgo: TimeSpan.FromDays(100));

        var response = await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{orderId}/photos/copies", scenario.Customer.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.photos_removed");
        Assert.True((await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId)).PhotosRemoved);
    }

    [Fact]
    public async Task OrderPhotoUploadUrl_GivesAKeyThatTheOrderAccepts()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var content = TestImages.Jpeg();

        var upload = await (await _client.SendJsonAsAsync(
            HttpMethod.Post,
            $"{Route}/photos/upload-url",
            scenario.Customer.AccessToken,
            new { contentType = "image/jpeg", contentLength = content.Length })).Content.ReadApiAsync<Application.Providers.Common.PhotoUploadResponse>();
        var body = new ByteArrayContent(content);
        body.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
        var stored = await _storage.PutAsync(upload.UploadUrl, body, TestContext.Current.CancellationToken);
        var order = await _client.CreateOrderOkAsync(scenario, factory.Tomorrow(), [upload.ObjectKey]);

        Assert.True(stored.IsSuccessStatusCode, $"{(int)stored.StatusCode}");
        Assert.Equal(OrderPhotoStatus.Processing, Assert.Single(order.Photos).Status);
    }

    private async Task<Guid> SeedDeclinedOrderWithStoredPhotosAsync(OrderScenario scenario, int photos)
    {
        var keys = Enumerable.Range(0, photos).Select(_ => OrderPhotoKeys.NewUpload(scenario.Customer.Id)).ToList();
        var orderId = await factory.SeedOrderAsync(scenario, uploadKeys: keys);

        await using var scope = factory.Services.CreateAsyncScope();
        var media = scope.ServiceProvider.GetRequiredService<IPrivateMediaStorage>();
        var order = (await scope.ServiceProvider.GetRequiredService<IOrderWritableRepository>().GetAsync(orderId, TestContext.Current.CancellationToken))!;
        foreach (var photo in order.Photos)
        {
            var objectKey = OrderPhotoKeys.Stored(order.Id, photo.Id);
            await media.SaveAsync(objectKey, TestImages.Jpeg(), "image/webp", TestContext.Current.CancellationToken);
            order.CompletePhoto(photo.Id, objectKey).ShouldSucceed();
        }

        order.Decline(DeclineReason.NoAvailability, null, factory.ClockAgo(TimeSpan.Zero)).ShouldSucceed();
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);
        return orderId;
    }
}
