using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed class Session
{
    public const int DeviceNameMaxLength = 100;

    public static readonly TimeSpan IdleLifetime = TimeSpan.FromDays(60);
    public static readonly TimeSpan AbsoluteLifetime = TimeSpan.FromDays(365);

    private Session()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string DeviceName { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset LastSeenAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public string? RevokedReason { get; private set; }

    public DateTimeOffset AbsoluteExpiresAt => CreatedAt + AbsoluteLifetime;

    public static Session Start(Guid userId, string? deviceName, IClock clock)
    {
        var now = clock.UtcNow;
        var name = FreeText.Normalize(deviceName);
        return new Session
        {
            Id = Guid.CreateVersion7(now),
            UserId = userId,
            DeviceName = name.Length == 0 ? "Aparelho" : name[..Math.Min(name.Length, DeviceNameMaxLength)],
            CreatedAt = now,
            LastSeenAt = now
        };
    }

    public void Revoke(string reason, IClock clock)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = clock.UtcNow;
        RevokedReason = reason;
    }
}
