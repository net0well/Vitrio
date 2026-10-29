using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed class RefreshToken
{
    private RefreshToken()
    {
    }

    public Guid Id { get; private set; }

    public Guid SessionId { get; private set; }

    public Guid FamilyId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset AbsoluteExpiresAt { get; private set; }

    public DateTimeOffset? UsedAt { get; private set; }

    public Guid? ReplacedById { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public static RefreshToken IssueFor(Session session, string tokenHash, IClock clock)
    {
        var now = clock.UtcNow;
        var idleExpiry = now + Session.IdleLifetime;
        return new RefreshToken
        {
            Id = Guid.CreateVersion7(now),
            SessionId = session.Id,
            FamilyId = session.Id,
            TokenHash = tokenHash,
            CreatedAt = now,
            ExpiresAt = idleExpiry < session.AbsoluteExpiresAt ? idleExpiry : session.AbsoluteExpiresAt,
            AbsoluteExpiresAt = session.AbsoluteExpiresAt
        };
    }
}
