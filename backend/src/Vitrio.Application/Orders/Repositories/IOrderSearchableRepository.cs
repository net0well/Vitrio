using Vitrio.Application.Orders.Common;

namespace Vitrio.Application.Orders.Repositories;

public interface IOrderSearchableRepository
{
    Task<OrderCustomerView?> GetCustomerViewAsync(Guid orderId, Guid customerId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<OrderCustomerPage> ListForCustomerAsync(Guid customerId, bool open, int skip, int take, CancellationToken cancellationToken);

    Task<OrderStoredPhotos?> FindStoredPhotosAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);

    Task<OrderSuggestionContext?> FindSuggestionContextAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken);
}
