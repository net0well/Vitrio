namespace Vitrio.Infrastructure.Persistence.Outbox;

public sealed class OutboxDelivery
{
    public const int MaxAttempts = 10;
    public const int LastErrorMaxLength = 200;

    private static readonly TimeSpan FirstRetryDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan MaxRetryDelay = TimeSpan.FromHours(1);

    private OutboxDelivery()
    {
    }

    public Guid MessageId { get; private set; }

    public string Handler { get; private set; } = string.Empty;

    public OutboxDeliveryStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public DateTimeOffset NextAttemptAt { get; private set; }

    public DateTimeOffset? LockedUntil { get; private set; }

    public string? LastError { get; private set; }

    public DateTimeOffset? ProcessedAt { get; private set; }

    public static OutboxDelivery Create(Guid messageId, string handler, DateTimeOffset now) => new()
    {
        MessageId = messageId,
        Handler = handler,
        Status = OutboxDeliveryStatus.Pending,
        NextAttemptAt = now
    };

    public void MarkSucceeded(DateTimeOffset now)
    {
        Status = OutboxDeliveryStatus.Succeeded;
        ProcessedAt = now;
        LockedUntil = null;
        LastError = null;
    }

    public void RegisterFailure(string error, DateTimeOffset now)
    {
        LastError = error.Length > LastErrorMaxLength ? error[..LastErrorMaxLength] : error;
        LockedUntil = null;

        if (Attempts >= MaxAttempts)
        {
            Status = OutboxDeliveryStatus.Failed;
            return;
        }

        var delay = FirstRetryDelay * Math.Pow(2, Math.Max(Attempts - 1, 0));
        NextAttemptAt = now + (delay < MaxRetryDelay ? delay : MaxRetryDelay);
    }
}
