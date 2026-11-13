using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Repositories;

public interface IOrderWritableRepository
{
    Task<Order?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);

    Task<Order?> GetForProviderAsync(Guid orderId, Guid providerId, CancellationToken cancellationToken);

    Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken);

    Task<int> CountOpenForCustomerAsync(Guid customerId, CancellationToken cancellationToken);

    Task<int> CountWaitingForCustomerAsync(Guid customerId, int categoryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListDueToExpireAsync(
        DateTimeOffset now,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListScheduledEndedBeforeAsync(
        DateTimeOffset endedBefore,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListWithPersonalDataEndedBeforeAsync(
        DateTimeOffset endedBefore,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Order>> ListAwaitingAnswerForUserAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Order order);
}
