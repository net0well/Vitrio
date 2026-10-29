using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Events;

public sealed class InvalidateProviderProfileCache(IReadCache cache, IProviderSearchableRepository providers)
    : IDomainEventHandler<ProviderPublished>,
        IDomainEventHandler<ProviderPaused>,
        IDomainEventHandler<ProviderProfileUpdated>,
        IDomainEventHandler<UserDeleted>
{
    public ValueTask Handle(ProviderPublished domainEvent, CancellationToken cancellationToken) =>
        InvalidateAsync(domainEvent.ProviderId, cancellationToken);

    public ValueTask Handle(ProviderPaused domainEvent, CancellationToken cancellationToken) =>
        InvalidateAsync(domainEvent.ProviderId, cancellationToken);

    public ValueTask Handle(ProviderProfileUpdated domainEvent, CancellationToken cancellationToken) =>
        InvalidateAsync(domainEvent.ProviderId, cancellationToken);

    public async ValueTask Handle(UserDeleted domainEvent, CancellationToken cancellationToken)
    {
        if (await providers.FindIdByUserAsync(domainEvent.UserId, cancellationToken) is { } providerId)
        {
            await InvalidateAsync(providerId, cancellationToken);
        }
    }

    private async ValueTask InvalidateAsync(Guid providerId, CancellationToken cancellationToken) =>
        await cache.RemoveTagAsync(ProviderCacheTags.Provider(providerId), cancellationToken);
}
