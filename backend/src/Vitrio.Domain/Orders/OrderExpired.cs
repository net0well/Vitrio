using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.expired", 1)]
public sealed record OrderExpired(Guid OrderId, Guid CustomerId, Guid ProviderId) : DomainEvent;
