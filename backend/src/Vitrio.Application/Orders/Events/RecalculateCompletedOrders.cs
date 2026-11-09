using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Orders.Services;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Events;

public sealed class RecalculateCompletedOrders(IProviderWritableRepository providers, IProviderStatsService stats, IUnitOfWork unitOfWork)
    : IDomainEventHandler<OrderCompleted>
{
    public async ValueTask Handle(OrderCompleted domainEvent, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByIdAsync(domainEvent.ProviderId, cancellationToken);
        if (provider is null)
        {
            return;
        }

        await stats.RefreshServicesDoneAsync(provider, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
