using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Common;

public static class AccountRoles
{
    public static readonly IReadOnlyCollection<string> Customer = [Roles.Customer];

    public static IReadOnlyCollection<string> For(bool isAdmin, bool isProvider) =>
    [
        Roles.Customer,
        .. isProvider ? [Roles.Provider] : Array.Empty<string>(),
        .. isAdmin ? [Roles.Admin] : Array.Empty<string>()
    ];
}
