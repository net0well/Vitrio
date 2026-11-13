using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.Services;

public interface IProviderOrderViewService
{
    Task<Result<OrderProviderView>> GetAsync(Guid orderId, CancellationToken cancellationToken);
}
