using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.RescheduleOrder;

public sealed class RescheduleOrderCommandHandler(
    ICurrentUser currentUser,
    IProviderOrderService providerOrders,
    IProviderOrderViewService views,
    IAuditLog audit,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RescheduleOrderCommand, Result<OrderProviderView>>
{
    public async ValueTask<Result<OrderProviderView>> Handle(RescheduleOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await providerOrders.LoadAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var rescheduled = order.Reschedule(command.Date!.Value, OrderTimes.ParseStart(command.Start)!.Value, command.DurationMinutes!.Value, clock);
        if (rescheduled.IsFailure)
        {
            return rescheduled.Error;
        }

        audit.Record(OrderAudit.Rescheduled, currentUser.Id, OrderAudit.Target, order.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var view = await views.GetAsync(order.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return view;
    }
}
