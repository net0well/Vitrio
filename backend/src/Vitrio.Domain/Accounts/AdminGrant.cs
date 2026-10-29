using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

public sealed class AdminGrant
{
    private AdminGrant()
    {
    }

    public Guid UserId { get; private set; }

    public DateTimeOffset GrantedAt { get; private set; }

    public static AdminGrant For(Guid userId, IClock clock) => new() { UserId = userId, GrantedAt = clock.UtcNow };
}
