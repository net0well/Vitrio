using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CancelProviderOrder;

public sealed class CancelProviderOrderCommandHandler(
    ICurrentUser currentUser,
    IProviderOrderService providerOrders,
    IProviderOrderViewService views,
    IAuditLog audit,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CancelProviderOrderCommand, Result<OrderProviderView>>
{
    public async ValueTask<Result<OrderProviderView>> Handle(CancelProviderOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await providerOrders.LoadAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var cancelled = order.Cancel(OrderParty.Provider, command.Reason!.Value, command.Details, clock);
        if (cancelled.IsFailure)
        {
            return cancelled.Error;
        }

        if (order.WasCancelledLate)
        {
            audit.Record(OrderAudit.LateCancellation, currentUser.Id, OrderAudit.Target, order.Id);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var view = await views.GetAsync(order.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return view;
    }
}
