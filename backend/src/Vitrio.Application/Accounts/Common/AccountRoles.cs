using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Common;

public static class AccountRoles
{
    public static readonly IReadOnlyCollection<string> Customer = [Roles.Customer];

    public static readonly IReadOnlyCollection<string> CustomerAndAdmin = [Roles.Customer, Roles.Admin];

    public static IReadOnlyCollection<string> For(bool isAdmin) => isAdmin ? CustomerAndAdmin : Customer;
}
