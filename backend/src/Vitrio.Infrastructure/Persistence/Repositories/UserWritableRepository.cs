using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class UserWritableRepository(AppDbContext db) : IUserWritableRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(user => user.Email == email.Value, cancellationToken);

    public Task<User?> GetPendingByEmailAsync(EmailAddress email, CancellationToken cancellationToken) =>
        db.Users.SingleOrDefaultAsync(
            user => user.Email == email.Value && user.Status == UserStatus.PendingVerification,
            cancellationToken);

    public void Add(User user) => db.Users.Add(user);
}
