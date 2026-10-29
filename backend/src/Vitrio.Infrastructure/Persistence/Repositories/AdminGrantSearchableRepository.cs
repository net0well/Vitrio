using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class AdminGrantSearchableRepository(AppDbContext db) : IAdminGrantSearchableRepository
{
    public Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken) =>
        db.AdminGrants.AsNoTracking().AnyAsync(grant => grant.UserId == userId, cancellationToken);
}
