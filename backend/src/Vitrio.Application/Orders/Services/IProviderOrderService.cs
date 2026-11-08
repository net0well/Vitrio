using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Services;

public interface IProviderOrderService
{
    Task<Guid?> FindCurrentProviderIdAsync(CancellationToken cancellationToken);

    Task<Order?> LoadAsync(Guid orderId, CancellationToken cancellationToken);
}
