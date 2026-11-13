using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("quote.declined", 1)]
public sealed record QuoteDeclined(Guid OrderId, Guid CustomerId, Guid ProviderId) : DomainEvent;
