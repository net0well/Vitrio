namespace Vitrio.Application.Orders.Common;

public static class OrderAudit
{
    public const string Target = "order";
    public const string AddressReleased = "order.address_released";
    public const string ContactRead = "order.contact_read";
    public const string Rescheduled = "order.rescheduled";
    public const string LateCancellation = "order.late_cancellation";
}
