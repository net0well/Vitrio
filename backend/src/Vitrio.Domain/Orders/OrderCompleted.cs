using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.completed", 1)]
public sealed record OrderCompleted(Guid OrderId, Guid CustomerId, Guid ProviderId, OrderParty CompletedBy) : DomainEvent;
