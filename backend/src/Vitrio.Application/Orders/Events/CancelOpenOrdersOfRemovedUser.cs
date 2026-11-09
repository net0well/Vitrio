using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.Events;

public sealed class CancelOpenOrdersOfRemovedUser(IOrderWritableRepository orders, IUnitOfWork unitOfWork, IClock clock)
    : IDomainEventHandler<UserDeleted>
{
    public async ValueTask Handle(UserDeleted domainEvent, CancellationToken cancellationToken)
    {
        var awaiting = await orders.ListAwaitingAnswerForUserAsync(domainEvent.UserId, cancellationToken);
        if (awaiting.Count == 0)
        {
            return;
        }

        foreach (var order in awaiting)
        {
            order.Cancel(OrderParty.System, CancellationReason.AccountDeleted, null, clock);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
