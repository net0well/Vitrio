namespace Vitrio.Application.Accounts.Repositories;

public interface IAdminGrantSearchableRepository
{
    Task<bool> IsAdminAsync(Guid userId, CancellationToken cancellationToken);
}
