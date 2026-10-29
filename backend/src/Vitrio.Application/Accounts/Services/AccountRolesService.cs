using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Providers.Repositories;

namespace Vitrio.Application.Accounts.Services;

internal sealed class AccountRolesService(IAdminGrantSearchableRepository adminGrants, IProviderSearchableRepository providers)
    : IAccountRolesService
{
    public async Task<IReadOnlyCollection<string>> ForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        AccountRoles.For(
            await adminGrants.IsAdminAsync(userId, cancellationToken),
            await providers.HasActiveProfileAsync(userId, cancellationToken));
}
