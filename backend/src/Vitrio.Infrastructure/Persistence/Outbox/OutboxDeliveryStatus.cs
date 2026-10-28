namespace Vitrio.Infrastructure.Persistence.Outbox;

public enum OutboxDeliveryStatus
{
    Pending,
    Succeeded,
    Failed
}
