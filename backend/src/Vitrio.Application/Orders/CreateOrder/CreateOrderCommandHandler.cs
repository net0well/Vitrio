using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CreateOrder;

public sealed class CreateOrderCommandHandler(
    ICurrentUser currentUser,
    IOrderRequestService requests,
    IOrderWritableRepository orders,
    ICustomerOrderViewService views,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateOrderCommand, Result<OrderCustomerView>>
{
    public async ValueTask<Result<OrderCustomerView>> Handle(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var request = await requests.PrepareAsync(currentUser.Id, command, cancellationToken);
        if (request.IsFailure)
        {
            return request.Error;
        }

        var order = Order.Create(currentUser.Id, command.ProviderId, request.Value, clock);
        if (order.IsFailure)
        {
            return order.Error;
        }

        orders.Add(order.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await views.GetAsync(order.Value.Id, cancellationToken);
    }
}
