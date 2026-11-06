using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CompleteCustomerOrder;

public sealed class CompleteCustomerOrderCommandHandler(
    ICurrentUser currentUser,
    IOrderWritableRepository orders,
    ICustomerOrderViewService views,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CompleteCustomerOrderCommand, Result<OrderCustomerView>>
{
    public async ValueTask<Result<OrderCustomerView>> Handle(CompleteCustomerOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetForCustomerAsync(command.OrderId, currentUser.Id, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var completed = order.Complete(OrderParty.Customer, clock);
        if (completed.IsFailure)
        {
            return completed.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await views.GetAsync(order.Id, cancellationToken);
    }
}
