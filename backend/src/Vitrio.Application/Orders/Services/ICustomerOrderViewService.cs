using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.Services;

public interface ICustomerOrderViewService
{
    Task<Result<OrderCustomerView>> GetAsync(Guid orderId, CancellationToken cancellationToken);
}
