using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.Services;

internal sealed class ProviderOrderViewService(
    ICurrentUser currentUser,
    IProviderOrderService providerOrders,
    IOrderSearchableRepository orders,
    IAuditLog audit,
    IClock clock) : IProviderOrderViewService
{
    public async Task<Result<OrderProviderView>> GetAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var providerId = await providerOrders.FindCurrentProviderIdAsync(cancellationToken);
        var view = providerId is { } id ? await orders.GetProviderViewAsync(orderId, id, clock.UtcNow, cancellationToken) : null;
        if (view is null)
        {
            return OrderErrors.NotFound;
        }

        if (view.Customer.Phone is not null)
        {
            audit.Record(OrderAudit.ContactRead, currentUser.Id, OrderAudit.Target, view.Id);
        }

        return view;
    }
}
