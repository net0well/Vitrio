using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Repositories;

public interface IOrderWritableRepository
{
    Task<Order?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);

    Task<Order?> GetForProviderAsync(Guid orderId, Guid providerId, CancellationToken cancellationToken);

    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    Task<int> CountOpenForCustomerAsync(Guid customerId, CancellationToken cancellationToken);

    Task<int> CountWaitingForCustomerAsync(Guid customerId, int categoryId, CancellationToken cancellationToken);

    void Add(Order order);
}
