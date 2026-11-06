using Vitrio.Application.Orders.CreateOrder;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.Services;

public interface IOrderRequestService
{
    Task<Result<OrderRequest>> PrepareAsync(Guid customerId, CreateOrderCommand command, CancellationToken cancellationToken);
}
