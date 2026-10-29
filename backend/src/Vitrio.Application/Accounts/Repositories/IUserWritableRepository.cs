using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IUserWritableRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByEmailAsync(EmailAddress email, CancellationToken cancellationToken);

    Task<User?> GetPendingByEmailAsync(EmailAddress email, CancellationToken cancellationToken);

    void Add(User user);
}
