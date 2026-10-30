using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("provider.profile_updated", 1)]
public sealed record ProviderProfileUpdated(Guid ProviderId) : DomainEvent;
