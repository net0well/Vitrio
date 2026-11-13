using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.rescheduled", 1)]
public sealed record OrderRescheduled(Guid OrderId, Guid CustomerId, Guid ProviderId, int RescheduleCount) : DomainEvent;
