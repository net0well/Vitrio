using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.cancelled", 1)]
public sealed record OrderCancelled(Guid OrderId, Guid CustomerId, Guid ProviderId, OrderParty CancelledBy, CancellationReason ReasonCode, bool WithinTwoHours) : DomainEvent;
