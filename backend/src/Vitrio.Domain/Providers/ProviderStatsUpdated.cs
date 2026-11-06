using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("provider.stats_updated", 1)]
public sealed record ProviderStatsUpdated(Guid ProviderId) : DomainEvent;
