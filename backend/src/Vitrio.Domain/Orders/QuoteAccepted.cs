using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("quote.accepted", 1)]
public sealed record QuoteAccepted(Guid OrderId, Guid CustomerId, Guid ProviderId, int QuoteVersion) : DomainEvent;
