using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("provider.paused", 1)]
public sealed record ProviderPaused(Guid ProviderId) : DomainEvent;
