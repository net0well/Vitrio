using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderCancellationResponse(OrderParty By, CancellationReason Reason, string? Details, DateTimeOffset At)
{
    public static OrderCancellationResponse? For(Order order, OrderParty viewer) =>
        order is { CancelledBy: { } by, CancellationReason: { } reason, CancelledAt: { } at }
            ? new OrderCancellationResponse(by, reason, by == viewer ? order.CancellationDetails : null, CityInstant.In(at, order.Place.TimeZoneId))
            : null;
}
