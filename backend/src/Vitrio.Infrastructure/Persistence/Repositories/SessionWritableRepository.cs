using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class SessionWritableRepository(AppDbContext db) : ISessionWritableRepository
{
    public Task<Session?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Sessions.SingleOrDefaultAsync(session => session.Id == id, cancellationToken);

    public Task<Session?> GetForUserAsync(Guid id, Guid userId, CancellationToken cancellationToken) =>
        db.Sessions.SingleOrDefaultAsync(session => session.Id == id && session.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<Session>> ListUnrevokedForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Sessions
            .Where(session => session.UserId == userId && session.RevokedAt == null)
            .ToListAsync(cancellationToken);

    public void Add(Session session) => db.Sessions.Add(session);
}
