using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Common;
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

    public async Task<MeResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await db.Users
            .AsNoTracking()
            .Where(account => account.Id == userId && account.Status == UserStatus.Active)
            .Select(account => new
            {
                account.Id,
                account.DisplayName,
                account.Email,
                account.PhoneNumber,
                IsAdmin = db.AdminGrants.Any(grant => grant.UserId == account.Id)
            })
            .SingleOrDefaultAsync(cancellationToken);

        return user is null
            ? null
            : MeResponse.Create(user.Id, user.DisplayName, user.Email, user.PhoneNumber, AccountRoles.For(user.IsAdmin));
    }
}
