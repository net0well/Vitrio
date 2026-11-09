using System.Net;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Search;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.CompleteForgottenOrders;
using Vitrio.Application.Orders.EraseOrderPersonalData;
using Vitrio.Application.Orders.ExpireOrders;
using Vitrio.Application.Orders.RecalculateProviderStats;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

[Collection(nameof(SharedDataJobsCollection))]
public sealed class OrderJobTests(ApiFactory factory)
{
    private const int SmallBatch = 2;

    private readonly HttpClient _client = factory.CreateAuthClient();
    private readonly HttpClient _storage = new();

    [Fact]
    public async Task ExpireOrders_WhenTheProviderDidNotAnswerInTime_ExpiresTheOrderAndSuggestsOthers()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var other = await factory.PublishAsync(_client, new SearchableProvider(scenario.Spot.Offset(2)) { DisplayName = "Rogério Alves" });
        var orderId = await factory.SeedOrderAsync(scenario, createdAgo: TimeSpan.FromDays(3));

        await SendAsync(new ExpireOrdersCommand(SmallBatch));

        var order = await _client.GetOrderAsync(scenario.Customer.AccessToken, orderId);
        var suggestions = await (await _client.SendAsAsync(HttpMethod.Get, $"{Route}/{orderId}/suggestions", scenario.Customer.AccessToken))
            .Content.ReadApiAsync<List<ProviderSearchItem>>();
        Assert.Equal(OrderStatus.Expired, order.Status);
        Assert.Equal([other.Id], suggestions.Select(item => item.Id));
    }

    [Fact]
    public async Task ExpireOrders_RunTwice_ExpiresAnOverdueQuoteOnceAndLeavesTheRest()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var threeDaysAgo = factory.ClockAgo(TimeSpan.FromDays(3));
        var overdue = await factory.SeedOrderAsync(
            scenario,
            order => order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(5)), threeDaysAgo).ShouldSucceed(),
            createdAgo: TimeSpan.FromDays(3));
        var waiting = await factory.SeedOrderAsync(scenario);

        await SendAsync(new ExpireOrdersCommand(SmallBatch));
        await SendAsync(new ExpireOrdersCommand(SmallBatch));

        Assert.Equal(OrderStatus.QuoteExpired, (await _client.GetOrderAsync(scenario.Customer.AccessToken, overdue)).Status);
        Assert.Equal(OrderStatus.Sent, (await _client.GetOrderAsync(scenario.Customer.AccessToken, waiting)).Status);
        Assert.Equal(1, await CountEventsAsync("quote.expired", overdue));
    }

    [Fact]
    public async Task CompleteForgottenOrders_SevenDaysAfterTheEnd_CompletesTheServiceBySystem()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var forgotten = await SeedScheduledAsync(scenario, createdAgo: TimeSpan.FromDays(10));
        var recent = await SeedScheduledAsync(scenario, createdAgo: TimeSpan.FromDays(8));

        await SendAsync(new CompleteForgottenOrdersCommand(SmallBatch));

        var completed = await _client.GetOrderAsync(scenario.Customer.AccessToken, forgotten);
        Assert.Equal((OrderStatus.Completed, OrderParty.System), (completed.Status, completed.CompletedBy));
        Assert.Equal(OrderStatus.Scheduled, (await _client.GetOrderAsync(scenario.Customer.AccessToken, recent)).Status);
        Assert.Equal(1, await CountEventsAsync("order.completed", forgotten));
    }

    [Fact]
    public async Task EraseOrderPersonalData_NinetyOneDaysAfterTheOrderClosed_KeepsOnlyTheNeighborhoodCityAndShortName()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var old = await SeedDeclinedWithStoredPhotoAsync(scenario, TimeSpan.FromDays(91));
        var recent = await SeedDeclinedWithStoredPhotoAsync(scenario, TimeSpan.FromDays(89));
        var photoUrl = Assert.Single((await _client.GetOrderAsync(scenario.Customer.AccessToken, old)).Photos).Url!;

        await SendAsync(new EraseOrderPersonalDataCommand(SmallBatch));
        await SendAsync(new EraseOrderPersonalDataCommand(SmallBatch));

        var erased = await StoredOrderAsync(old);
        Assert.Null(erased.Address);
        Assert.Null(erased.Contact);
        Assert.NotNull(erased.PersonalDataErasedAt);
        Assert.Equal(("Bruno C.", "Bairro de Teste", SearchableProvider.FarAway.Name), (erased.CustomerDisplayName, erased.Place.Neighborhood, erased.Place.CityName));
        Assert.Equal(OrderPhotoStatus.Removed, Assert.Single(erased.Photos).Status);
        var view = await _client.GetOrderAsync(scenario.Customer.AccessToken, old);
        Assert.True(view.PhotosRemoved);
        Assert.Empty(view.Photos);
        Assert.Equal(HttpStatusCode.NotFound, (await _storage.GetAsync(photoUrl, TestContext.Current.CancellationToken)).StatusCode);
        Assert.Equal(1, await factory.QueryAsync(db => db.AuditEvents.CountAsync(
            audit => audit.Action == OrderAudit.PersonalDataErased && audit.TargetId == old,
            TestContext.Current.CancellationToken)));
        var kept = await StoredOrderAsync(recent);
        Assert.NotNull(kept.Contact);
        Assert.Null(kept.PersonalDataErasedAt);
        Assert.Equal(OrderPhotoStatus.Ready, Assert.Single(kept.Photos).Status);
    }

    [Fact]
    public async Task RecalculateProviderStats_WhenTheAnsweredOrdersLeaveTheWindow_ClearsTheResponseTime()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var then = factory.ClockAgo(TimeSpan.FromDays(91));
        for (var index = 0; index < 5; index++)
        {
            await factory.SeedOrderAsync(
                scenario,
                order =>
                {
                    order.Decline(DeclineReason.NoAvailability, null, then).ShouldSucceed();
                    order.ClearDomainEvents();
                },
                createdAgo: TimeSpan.FromDays(91));
        }

        await factory.QueryAsync(db => db.Database.ExecuteSqlAsync(
            $"update providers set response_time_bucket = '15min', response_sample_size = 5 where id = {scenario.Provider.Id}",
            TestContext.Current.CancellationToken));

        await SendAsync(new RecalculateProviderStatsCommand());

        var provider = await factory.QueryAsync(db => db.Providers.AsNoTracking()
            .SingleAsync(candidate => candidate.Id == scenario.Provider.Id, TestContext.Current.CancellationToken));
        Assert.Null(provider.ResponseTimeBucket);
        Assert.Equal(0, provider.ResponseSampleSize);
    }

    private Task<Guid> SeedScheduledAsync(OrderScenario scenario, TimeSpan createdAgo)
    {
        var then = factory.ClockAgo(createdAgo);
        return factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(then).AddDays(2), startHour: 10), then).ShouldSucceed();
                order.AcceptQuote(1, then).ShouldSucceed();
            },
            createdAgo: createdAgo);
    }

    private async Task<Guid> SeedDeclinedWithStoredPhotoAsync(OrderScenario scenario, TimeSpan closedAgo)
    {
        var orderId = await factory.SeedOrderAsync(scenario, uploadKeys: [OrderPhotoKeys.NewUpload(scenario.Customer.Id)], createdAgo: closedAgo);

        await using var scope = factory.Services.CreateAsyncScope();
        var order = (await scope.ServiceProvider.GetRequiredService<IOrderWritableRepository>().GetAsync(orderId, TestContext.Current.CancellationToken))!;
        var photo = Assert.Single(order.Photos);
        var objectKey = OrderPhotoKeys.Stored(order.Id, photo.Id);
        await scope.ServiceProvider.GetRequiredService<IPrivateMediaStorage>()
            .SaveAsync(objectKey, TestImages.Jpeg(), "image/webp", TestContext.Current.CancellationToken);
        order.CompletePhoto(photo.Id, objectKey).ShouldSucceed();
        order.Decline(DeclineReason.NoAvailability, null, factory.ClockAgo(closedAgo)).ShouldSucceed();
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);
        return orderId;
    }

    private Task<Order> StoredOrderAsync(Guid orderId) =>
        factory.QueryAsync(db => db.Orders.AsNoTracking()
            .Include(order => order.Photos)
            .SingleAsync(order => order.Id == orderId, TestContext.Current.CancellationToken));

    private async Task<int> CountEventsAsync(string type, Guid orderId)
    {
        var payloads = await factory.QueryAsync(db => db.OutboxMessages
            .Where(message => message.Type == type)
            .Select(message => message.Payload)
            .ToListAsync(TestContext.Current.CancellationToken));
        return payloads.Count(payload => payload.Contains(orderId.ToString(), StringComparison.Ordinal));
    }

    private async Task SendAsync(ICommand<Result> command)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        (await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken)).ShouldSucceed();
    }
}
