using Vitrio.Domain.Common;

namespace Vitrio.Domain.Orders;

[EventType("order.address_released", 1)]
public sealed record AddressReleased(Guid OrderId, Guid ProviderId) : DomainEvent;
