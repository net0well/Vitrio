using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("mei.verified", 1)]
public sealed record MeiVerified(Guid ProviderId, bool FromPending) : DomainEvent;
