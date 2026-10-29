using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IAdminGrantWritableRepository
{
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken);

    void Add(AdminGrant grant);
}
