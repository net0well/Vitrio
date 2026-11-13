using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Orders.Events;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Orders;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence.Outbox;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class ProviderOrderStatsTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task SendQuote_AfterFourAnsweredOrders_ShowsTheResponseTimeOnTheProfile()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var fiveMinutesAgo = factory.ClockAgo(TimeSpan.FromMinutes(5));
        for (var index = 0; index < 4; index++)
        {
            await factory.SeedOrderAsync(
                scenario,
                order => order.Decline(DeclineReason.NoAvailability, null, fiveMinutesAgo).ShouldSucceed(),
                createdAgo: TimeSpan.FromMinutes(10));
        }

        var waiting = await factory.SeedOrderAsync(scenario, createdAgo: TimeSpan.FromMinutes(10));

        var quote = await _client.SendQuoteAsync(scenario.Provider.AccessToken, waiting, QuoteBody(factory.Tomorrow().AddDays(1)));

        Assert.Equal(HttpStatusCode.OK, quote.StatusCode);
        await factory.UntilAsync(async () => (await ProviderAsync(scenario.Provider.Id)) is { ResponseTimeBucket: "15min", ResponseSampleSize: 5 });
        await factory.UntilAsync(async () => (await PublicProfileAsync(scenario.Provider.Id)).ResponseTime == "15min");
    }

    [Fact]
    public async Task CompleteOrder_CountsTheServiceDoneForTheProvider()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var threeDaysAgo = factory.ClockAgo(TimeSpan.FromDays(3));
        var started = await factory.SeedOrderAsync(
            scenario,
            order =>
            {
                order.SendQuote(Terms(LocalToday(threeDaysAgo).AddDays(1), startHour: 10), threeDaysAgo).ShouldSucceed();
                order.AcceptQuote(1, threeDaysAgo).ShouldSucceed();
            },
            createdAgo: TimeSpan.FromDays(3));

        var response = await _client.SendAsAsync(HttpMethod.Post, $"{Route}/{started}/complete", scenario.Customer.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.UntilAsync(async () => (await ProviderAsync(scenario.Provider.Id)).CompletedOrders == 1);
    }

    [Fact]
    public async Task ProviderCancellation_InsideTwoHours_CountsALateCancellation()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var threeHoursAgo = factory.ClockAgo(TimeSpan.FromHours(3));
        var local = CityTimeOf(factory.ClockAgo(TimeSpan.FromMinutes(-90)).UtcNow);
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
            new { reason = "unforeseen" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.UntilAsync(async () => (await ProviderAsync(scenario.Provider.Id)).LateCancellations90d == 1);
    }

    [Fact]
    public async Task CustomerCancellation_InsideTwoHours_DoesNotCountAgainstTheProvider()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var threeHoursAgo = factory.ClockAgo(TimeSpan.FromHours(3));
        var local = CityTimeOf(factory.ClockAgo(TimeSpan.FromMinutes(-90)).UtcNow);
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
            $"{Route}/{orderId}/cancel",
            scenario.Customer.AccessToken,
            new { reason = "timeNoLongerWorks" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await factory.UntilAsync(() => HandledAsync("order.cancelled", orderId, nameof(RecordLateCancellation)));
        Assert.Equal(0, (await ProviderAsync(scenario.Provider.Id)).LateCancellations90d);
    }

    private async Task<bool> HandledAsync(string eventType, Guid orderId, string handler)
    {
        var messages = await factory.QueryAsync(db => db.OutboxMessages
            .Where(message => message.Type == eventType)
            .Select(message => new { message.Id, message.Payload })
            .ToListAsync(TestContext.Current.CancellationToken));
        var messageId = messages.Single(message => message.Payload.Contains(orderId.ToString(), StringComparison.Ordinal)).Id;
        return await factory.QueryAsync(db => db.OutboxDeliveries.AnyAsync(
            delivery => delivery.MessageId == messageId && delivery.Handler == handler && delivery.Status == OutboxDeliveryStatus.Succeeded,
            TestContext.Current.CancellationToken));
    }

    private Task<Provider> ProviderAsync(Guid providerId) =>
        factory.QueryAsync(db => db.Providers.AsNoTracking().SingleAsync(provider => provider.Id == providerId, TestContext.Current.CancellationToken));

    private async Task<ProviderPublicResponse> PublicProfileAsync(Guid providerId) =>
        await (await _client.GetAsync($"/api/v1/providers/{providerId}", TestContext.Current.CancellationToken)).Content.ReadApiAsync<ProviderPublicResponse>();
}
