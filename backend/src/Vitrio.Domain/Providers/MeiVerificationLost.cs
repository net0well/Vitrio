using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("mei.verification_lost", 1)]
public sealed record MeiVerificationLost(Guid ProviderId) : DomainEvent;
