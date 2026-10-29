using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IUserSearchableRepository
{
    Task<string?> FindPendingDisplayNameAsync(EmailAddress email, CancellationToken cancellationToken);
}
