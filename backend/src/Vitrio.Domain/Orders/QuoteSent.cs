using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("quote.sent", 1)]
public sealed record QuoteSent(Guid OrderId, Guid CustomerId, Guid ProviderId, int QuoteVersion) : DomainEvent;
