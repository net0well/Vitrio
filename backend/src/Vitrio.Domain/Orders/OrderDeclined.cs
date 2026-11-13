using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.declined", 1)]
public sealed record OrderDeclined(Guid OrderId, Guid CustomerId, Guid ProviderId, DeclineReason ReasonCode) : DomainEvent;
