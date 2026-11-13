using System.Net;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Domain.Orders;
using static Vitrio.Api.IntegrationTests.Orders.OrderApi;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class OrderAccountDeletionTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task DeleteAccount_WithAScheduledServiceAsCustomer_ReturnsConflict()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        await SeedScheduledAsync(scenario);

        var response = await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", scenario.Customer.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.scheduled_blocks_deletion");
        Assert.Equal(HttpStatusCode.OK, (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", scenario.Customer.AccessToken)).StatusCode);
    }

    [Fact]
    public async Task DeleteAccount_WithAScheduledServiceAsProvider_ReturnsConflict()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        await SeedScheduledAsync(scenario);

        var response = await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", scenario.Provider.AccessToken);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "order.scheduled_blocks_deletion");
    }

    [Fact]
    public async Task DeleteAccount_OfACustomerWithoutScheduledServices_CancelsTheOrdersAwaitingAnAnswer()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var now = factory.ClockAgo(TimeSpan.Zero);
        var sent = await factory.SeedOrderAsync(scenario);
        var quoted = await factory.SeedOrderAsync(scenario, order => order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed());
        var declined = await factory.SeedOrderAsync(scenario, order => order.Decline(DeclineReason.NoAvailability, null, now).ShouldSucceed());

        var response = await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", scenario.Customer.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await factory.UntilAsync(async () => await StatusAsync(sent) == OrderStatus.Cancelled && await StatusAsync(quoted) == OrderStatus.Cancelled);
        var cancelled = await factory.QueryAsync(db => db.Orders.AsNoTracking()
            .Where(order => order.Id == sent || order.Id == quoted)
            .Select(order => new { order.CancelledBy, order.CancellationReason })
            .ToListAsync(TestContext.Current.CancellationToken));
        Assert.All(cancelled, order => Assert.Equal((OrderParty.System, CancellationReason.AccountDeleted), (order.CancelledBy!.Value, order.CancellationReason!.Value)));
        Assert.Equal(OrderStatus.Declined, await StatusAsync(declined));
    }

    [Fact]
    public async Task DeleteAccount_OfAProviderWithoutScheduledServices_CancelsTheOrdersAwaitingAnAnswer()
    {
        var scenario = await factory.StartOrderScenarioAsync(_client);
        var sent = await factory.SeedOrderAsync(scenario);

        var response = await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", scenario.Provider.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        await factory.UntilAsync(async () => await StatusAsync(sent) == OrderStatus.Cancelled);
        Assert.Equal(CancellationReason.AccountDeleted, (await _client.GetOrderAsync(scenario.Customer.AccessToken, sent)).Cancellation!.Reason);
    }

    private Task<Guid> SeedScheduledAsync(OrderScenario scenario)
    {
        var now = factory.ClockAgo(TimeSpan.Zero);
        return factory.SeedOrderAsync(scenario, order =>
        {
            order.SendQuote(Terms(LocalToday(now).AddDays(3)), now).ShouldSucceed();
            order.AcceptQuote(1, now).ShouldSucceed();
        });
    }

    private Task<OrderStatus> StatusAsync(Guid orderId) =>
        factory.QueryAsync(db => db.Orders.Where(order => order.Id == orderId).Select(order => order.Status)
            .SingleAsync(TestContext.Current.CancellationToken));
}
