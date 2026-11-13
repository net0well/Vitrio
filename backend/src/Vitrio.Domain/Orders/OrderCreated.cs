using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.created", 1)]
public sealed record OrderCreated(Guid OrderId, Guid CustomerId, Guid ProviderId) : DomainEvent;
