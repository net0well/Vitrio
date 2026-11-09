using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Orders.Services;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Events;

public sealed class RecordLateCancellation(IProviderWritableRepository providers, IProviderStatsService stats, IUnitOfWork unitOfWork)
    : IDomainEventHandler<OrderCancelled>
{
    public async ValueTask Handle(OrderCancelled domainEvent, CancellationToken cancellationToken)
    {
        if (domainEvent is not { CancelledBy: OrderParty.Provider, WithinTwoHours: true })
        {
            return;
        }

        var provider = await providers.GetByIdAsync(domainEvent.ProviderId, cancellationToken);
        if (provider is null)
        {
            return;
        }

        await stats.RefreshLateCancellationsAsync(provider, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
