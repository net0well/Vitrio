using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

[EventType("mei.verification_failed", 1)]
public sealed record MeiVerificationFailed(Guid ProviderId, string ReasonCode, bool FromPending) : DomainEvent;
