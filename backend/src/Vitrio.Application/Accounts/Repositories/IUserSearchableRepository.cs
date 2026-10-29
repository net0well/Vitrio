using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IUserSearchableRepository
{
    Task<string?> FindDisplayNameAsync(EmailAddress email, UserStatus status, CancellationToken cancellationToken);

    Task<MeResponse?> GetMeAsync(Guid userId, CancellationToken cancellationToken);
}
