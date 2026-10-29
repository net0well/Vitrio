using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class RefreshTokenWritableRepository(AppDbContext db) : IRefreshTokenWritableRepository
{
    public void Add(RefreshToken refreshToken) => db.RefreshTokens.Add(refreshToken);
}
