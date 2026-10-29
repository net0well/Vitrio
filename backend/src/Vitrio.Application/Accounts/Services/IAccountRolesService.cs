namespace Vitrio.Application.Accounts.Services;

public interface IAccountRolesService
{
    Task<IReadOnlyCollection<string>> ForUserAsync(Guid userId, CancellationToken cancellationToken);
}
