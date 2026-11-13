using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Accounts;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Api.IntegrationTests.Providers;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Time;

namespace Vitrio.Api.IntegrationTests.Persistence;

public sealed class OrderPersistenceTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task ScheduledOrder_IsReadBackWithQuoteScheduleAndEncryptedContact()
    {
        var (customerId, providerId) = await SeedPartiesAsync();
        var orderId = await SaveAsync(customerId, providerId, (order, clock) =>
        {
            order.SendQuote(Terms(clock), clock);
            order.AcceptQuote(1, clock);
        });

        await using var scope = factory.Services.CreateAsyncScope();
        var order = await scope.ServiceProvider.GetRequiredService<IOrderWritableRepository>()
            .GetForCustomerAsync(orderId, customerId, TestContext.Current.CancellationToken);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Scheduled, order.Status);
        Assert.Equal(18050, order.Quote!.Amount.Cents);
        Assert.Equal(new TimeOnly(14, 0), order.Quote.Schedule.Start);
        Assert.Equal(order.Quote.Schedule.StartsAt, order.Schedule!.StartsAt);
        Assert.Equal("Rua Rui Barbosa", order.Address!.Street);
        Assert.Equal("Mariana Souza", order.Contact!.FullName);
        Assert.Equal("Centro", order.Place.Neighborhood);
        Assert.Equal(2, order.Photos.Count);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var stored = await db.Database
            .SqlQuery<string>($"select address_street || ' ' || customer_full_name || ' ' || customer_phone as \"Value\" from orders where id = {orderId}")
            .SingleAsync(TestContext.Current.CancellationToken);
        Assert.DoesNotContain("Rui Barbosa", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("Mariana", stored, StringComparison.Ordinal);
        Assert.DoesNotContain("991234567", stored, StringComparison.Ordinal);
    }

    [Fact]
    public async Task OrderOfAnotherCustomer_IsNotFound()
    {
        var (customerId, providerId) = await SeedPartiesAsync();
        var orderId = await SaveAsync(customerId, providerId, (_, _) => { });

        await using var scope = factory.Services.CreateAsyncScope();
        var orders = scope.ServiceProvider.GetRequiredService<IOrderWritableRepository>();
        Assert.Null(await orders.GetForCustomerAsync(orderId, Guid.NewGuid(), TestContext.Current.CancellationToken));
        Assert.Null(await orders.GetForProviderAsync(orderId, Guid.NewGuid(), TestContext.Current.CancellationToken));
        Assert.NotNull(await orders.GetForProviderAsync(orderId, providerId, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TwoChangesToTheSameOrder_TheSecondConflicts()
    {
        var (customerId, providerId) = await SeedPartiesAsync();
        var orderId = await SaveAsync(customerId, providerId, (order, clock) => order.SendQuote(Terms(clock), clock));

        await using var first = factory.Services.CreateAsyncScope();
        await using var second = factory.Services.CreateAsyncScope();
        var clock = first.ServiceProvider.GetRequiredService<IClock>();
        var accepted = await first.ServiceProvider.GetRequiredService<IOrderWritableRepository>().GetAsync(orderId, TestContext.Current.CancellationToken);
        var cancelled = await second.ServiceProvider.GetRequiredService<IOrderWritableRepository>().GetAsync(orderId, TestContext.Current.CancellationToken);
        accepted!.AcceptQuote(1, clock);
        cancelled!.Cancel(OrderParty.Customer, CancellationReason.ResolvedAnotherWay, null, clock);
        await first.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ConcurrencyConflictException>(() =>
            second.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken));
    }

    private static QuoteTerms Terms(IClock clock) =>
        new(QuoteType.FixedPrice, 180.5m, DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(3), new TimeOnly(14, 0), 60, true, "disjuntor 40 A", null);

    private async Task<(Guid CustomerId, Guid ProviderId)> SeedPartiesAsync()
    {
        var customer = await factory.SeedActiveUserAsync($"cliente-{Guid.NewGuid():N}@exemplo.com.br", "Mariana Souza");
        var accessToken = await factory.SignInAsProviderAsync(_client);
        var provider = await _client.GetProviderAsync(accessToken);
        return (customer.Id, provider.Id);
    }

    private async Task<Guid> SaveAsync(Guid customerId, Guid providerId, Action<Order, IClock> change)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var request = new OrderRequest(
            null,
            "Chuveiro",
            1,
            "Chuveiro desarmando o disjuntor",
            "O chuveiro desarma o disjuntor quando ligo no modo inverno.",
            DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddDays(3),
            PreferredPeriod.Afternoon,
            new OrderAddress("Rua Rui Barbosa", "123", "apto 12", "14870-000"),
            new OrderContact("Mariana Souza", "+5516991234567"),
            new OrderPlace(3524303, "Jaboticabal", "SP", "Centro", "America/Sao_Paulo", -21.2547, -48.3221, false),
            ["uploads/foto-1", "uploads/foto-2"]);
        var order = Order.Create(customerId, providerId, request, clock).Value;
        change(order, clock);
        scope.ServiceProvider.GetRequiredService<IOrderWritableRepository>().Add(order);
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(TestContext.Current.CancellationToken);
        return order.Id;
    }
}
