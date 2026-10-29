using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class UserSearchableRepository(AppDbContext db) : IUserSearchableRepository
{
    public Task<string?> FindDisplayNameAsync(EmailAddress email, UserStatus status, CancellationToken cancellationToken) =>
        db.Users
            .AsNoTracking()
            .Where(user => user.Email == email.Value && user.Status == status)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
}
