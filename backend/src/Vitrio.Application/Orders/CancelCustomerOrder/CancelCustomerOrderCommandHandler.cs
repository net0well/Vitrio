using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CancelCustomerOrder;

public sealed class CancelCustomerOrderCommandHandler(
    ICurrentUser currentUser,
    IOrderWritableRepository orders,
    ICustomerOrderViewService views,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CancelCustomerOrderCommand, Result<OrderCustomerView>>
{
    public async ValueTask<Result<OrderCustomerView>> Handle(CancelCustomerOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetForCustomerAsync(command.OrderId, currentUser.Id, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var cancelled = order.Cancel(OrderParty.Customer, command.Reason!.Value, command.Details, clock);
        if (cancelled.IsFailure)
        {
            return cancelled.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await views.GetAsync(order.Id, cancellationToken);
    }
}
