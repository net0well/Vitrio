using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.AcceptQuote;

public sealed class AcceptQuoteCommandHandler(
    ICurrentUser currentUser,
    IOrderWritableRepository orders,
    IAuditLog audit,
    ICustomerOrderViewService views,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AcceptQuoteCommand, Result<OrderCustomerView>>
{
    public async ValueTask<Result<OrderCustomerView>> Handle(AcceptQuoteCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetForCustomerAsync(command.OrderId, currentUser.Id, cancellationToken);
        if (order is null)
        {
            return OrderErrors.NotFound;
        }

        var accepted = order.AcceptQuote(command.QuoteVersion, clock);
        if (accepted.IsFailure)
        {
            return accepted.Error;
        }

        audit.Record(OrderAudit.AddressReleased, currentUser.Id, OrderAudit.Target, order.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return await views.GetAsync(order.Id, cancellationToken);
    }
}
