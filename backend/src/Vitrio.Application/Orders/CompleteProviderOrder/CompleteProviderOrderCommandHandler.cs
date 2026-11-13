using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CompleteProviderOrder;

public sealed class CompleteProviderOrderCommandHandler(
    IProviderOrderService providerOrders,
    IProviderOrderViewService views,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CompleteProviderOrderCommand, Result<OrderProviderView>>
{
    public async ValueTask<Result<OrderProviderView>> Handle(CompleteProviderOrderCommand command, CancellationToken cancellationToken)
    {
        var order = await providerOrders.LoadAsync(command.OrderId, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var completed = order.Complete(OrderParty.Provider, clock);
        if (completed.IsFailure)
        {
            return completed.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        var view = await views.GetAsync(order.Id, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return view;
    }
}
