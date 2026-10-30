using Vitrio.Shared.Time;

namespace Vitrio.Domain.Auditing;

public sealed class AuditEvent
{
    private AuditEvent()
    {
    }

    public Guid Id { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    public Guid? ActorId { get; private set; }

    public string Action { get; private set; } = string.Empty;

    public string TargetType { get; private set; } = string.Empty;

    public Guid? TargetId { get; private set; }

    public string? IpPrefix { get; private set; }

    public string? TraceId { get; private set; }

    public static AuditEvent Record(
        string action,
        Guid? actorId,
        string targetType,
        Guid? targetId,
        string? ipPrefix,
        string? traceId,
        IClock clock)
    {
        var now = clock.UtcNow;
        return new AuditEvent
        {
            Id = Guid.CreateVersion7(now),
            OccurredAt = now,
            ActorId = actorId,
            Action = action,
            TargetType = targetType,
            TargetId = targetId,
            IpPrefix = ipPrefix,
            TraceId = traceId
        };
    }
}
