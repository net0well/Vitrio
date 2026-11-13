using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.Services;

internal sealed class CustomerOrderViewService(ICurrentUser currentUser, IOrderSearchableRepository orders, IClock clock) : ICustomerOrderViewService
{
    public async Task<Result<OrderCustomerView>> GetAsync(Guid orderId, CancellationToken cancellationToken) =>
        await orders.GetCustomerViewAsync(orderId, currentUser.Id, clock.UtcNow, cancellationToken) is { } view
            ? view
            : OrderErrors.NotFound;
}
