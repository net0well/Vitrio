namespace Vitrio.Domain.Orders;

public enum OrderStatus
{
    Sent,
    Declined,
    Expired,
    Quoted,
    QuoteDeclined,
    QuoteExpired,
    Scheduled,
    Cancelled,
    Completed,
    PaymentConfirmed,
    PaymentDisputed,
    PaymentVoided
}
