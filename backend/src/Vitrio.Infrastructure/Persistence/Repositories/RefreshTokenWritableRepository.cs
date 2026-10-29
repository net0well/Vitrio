using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenWritableRepository(AppDbContext db) : IRefreshTokenWritableRepository
{
    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        db.RefreshTokens.SingleOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> ListUnrevokedForSessionsAsync(
        IReadOnlyCollection<Guid> sessionIds,
        CancellationToken cancellationToken) =>
        await db.RefreshTokens
            .Where(token => sessionIds.Contains(token.SessionId) && token.RevokedAt == null)
            .ToListAsync(cancellationToken);

    public void Add(RefreshToken refreshToken) => db.RefreshTokens.Add(refreshToken);
}
