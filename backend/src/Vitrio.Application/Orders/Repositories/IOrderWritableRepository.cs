using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Repositories;

public interface IOrderWritableRepository
{
    Task<Order?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);

    Task<Order?> GetForProviderAsync(Guid orderId, Guid providerId, CancellationToken cancellationToken);

    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    void Add(Order order);
}
