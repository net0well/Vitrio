using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.Services;

internal sealed class ProviderStatsService(IOrderSearchableRepository orders, IClock clock) : IProviderStatsService
{
    public async Task RefreshResponseTimeAsync(Provider provider, CancellationToken cancellationToken) =>
        provider.UpdateResponseTime(await orders.ListResponseDelaysAsync(
            provider.Id,
            clock.UtcNow - ProviderStats.Window,
            ProviderStats.ResponseSampleSize,
            cancellationToken));

    public async Task RefreshServicesDoneAsync(Provider provider, CancellationToken cancellationToken) =>
        provider.UpdateCompletedOrders(await orders.CountServicesDoneAsync(provider.Id, cancellationToken));

    public async Task RefreshLateCancellationsAsync(Provider provider, CancellationToken cancellationToken) =>
        provider.UpdateLateCancellations(await orders.CountLateCancellationsAsync(provider.Id, clock.UtcNow - ProviderStats.Window, cancellationToken));
}
