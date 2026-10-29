using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("provider.published", 1)]
public sealed record ProviderPublished(Guid ProviderId) : DomainEvent;
