namespace Vitrio.Infrastructure.Persistence.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public int Version { get; private set; }

    public string Payload { get; private set; } = string.Empty;

    public DateTimeOffset OccurredAt { get; private set; }

    public static OutboxMessage Create(Guid eventId, string type, int version, string payload, DateTimeOffset occurredAt) => new()
    {
        Id = eventId,
        Type = type,
        Version = version,
        Payload = payload,
        OccurredAt = occurredAt
    };
}
