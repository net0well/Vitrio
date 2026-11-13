using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("quote.expired", 1)]
public sealed record QuoteExpired(Guid OrderId, Guid CustomerId, Guid ProviderId) : DomainEvent;
