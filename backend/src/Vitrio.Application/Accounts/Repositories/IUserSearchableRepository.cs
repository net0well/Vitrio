using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IUserSearchableRepository
{
    Task<string?> FindDisplayNameAsync(EmailAddress email, UserStatus status, CancellationToken cancellationToken);
}
