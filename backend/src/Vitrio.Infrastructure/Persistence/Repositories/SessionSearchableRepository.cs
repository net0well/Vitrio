using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.ListSessions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class SessionSearchableRepository(AppDbContext db) : ISessionSearchableRepository
{
    public async Task<IReadOnlyList<SessionResponse>> ListActiveForUserAsync(
        Guid userId,
        Guid currentSessionId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var idleSince = now - Session.IdleLifetime;
        var createdSince = now - Session.AbsoluteLifetime;
        return await db.Sessions
            .AsNoTracking()
            .Where(session => session.UserId == userId
                && session.RevokedAt == null
                && session.LastSeenAt > idleSince
                && session.CreatedAt > createdSince)
            .OrderByDescending(session => session.LastSeenAt)
            .Select(session => new SessionResponse(
                session.Id,
                session.DeviceName,
                session.CreatedAt,
                session.LastSeenAt,
                session.Id == currentSessionId))
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsActiveAsync(Guid sessionId, CancellationToken cancellationToken) =>
        db.Sessions
            .AsNoTracking()
            .AnyAsync(
                session => session.Id == sessionId
                    && session.RevokedAt == null
                    && db.Users.Any(user => user.Id == session.UserId && user.Status == UserStatus.Active),
                cancellationToken);
}
