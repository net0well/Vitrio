using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Services;

internal sealed class ProviderOrderService(
    ICurrentUser currentUser,
    IProviderSearchableRepository providers,
    IOrderWritableRepository orders) : IProviderOrderService
{
    public Task<Guid?> FindCurrentProviderIdAsync(CancellationToken cancellationToken) =>
        providers.FindIdByUserAsync(currentUser.Id, cancellationToken);

    public async Task<Order?> LoadAsync(Guid orderId, CancellationToken cancellationToken) =>
        await FindCurrentProviderIdAsync(cancellationToken) is { } providerId
            ? await orders.GetForProviderAsync(orderId, providerId, cancellationToken)
            : null;
}
