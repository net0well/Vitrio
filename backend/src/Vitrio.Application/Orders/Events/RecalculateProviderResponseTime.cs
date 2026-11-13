using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Orders.Services;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Events;

public sealed class RecalculateProviderResponseTime(IProviderWritableRepository providers, IProviderStatsService stats, IUnitOfWork unitOfWork)
    : IDomainEventHandler<QuoteSent>, IDomainEventHandler<OrderDeclined>, IDomainEventHandler<OrderExpired>
{
    public ValueTask Handle(QuoteSent domainEvent, CancellationToken cancellationToken) =>
        domainEvent.QuoteVersion == 1 ? RecalculateAsync(domainEvent.ProviderId, cancellationToken) : ValueTask.CompletedTask;

    public ValueTask Handle(OrderDeclined domainEvent, CancellationToken cancellationToken) =>
        RecalculateAsync(domainEvent.ProviderId, cancellationToken);

    public ValueTask Handle(OrderExpired domainEvent, CancellationToken cancellationToken) =>
        RecalculateAsync(domainEvent.ProviderId, cancellationToken);

    private async ValueTask RecalculateAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByIdAsync(providerId, cancellationToken);
        if (provider is null)
        {
            return;
        }

        await stats.RefreshResponseTimeAsync(provider, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
