using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class AdminGrantWritableRepository(AppDbContext db) : IAdminGrantWritableRepository
{
    public Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken) =>
        db.AdminGrants.AnyAsync(grant => grant.UserId == userId, cancellationToken);

    public void Add(AdminGrant grant) => db.AdminGrants.Add(grant);
}
