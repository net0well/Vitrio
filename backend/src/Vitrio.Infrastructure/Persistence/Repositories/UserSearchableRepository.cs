using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class UserSearchableRepository(AppDbContext db) : IUserSearchableRepository
{
    public Task<string?> FindPendingDisplayNameAsync(EmailAddress email, CancellationToken cancellationToken) =>
        db.Users
            .AsNoTracking()
            .Where(user => user.Email == email.Value && user.Status == UserStatus.PendingVerification)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
}
