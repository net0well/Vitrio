using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Common;

public sealed record MeResponse(
    Guid Id,
    string DisplayName,
    string Initials,
    string Email,
    string? PhoneNumber,
    IReadOnlyCollection<string> Roles,
    bool HasProviderProfile)
{
    public static MeResponse From(User user, IReadOnlyCollection<string> roles) => new(
        user.Id,
        user.DisplayName,
        Domain.Accounts.DisplayName.FromStored(user.DisplayName).Initials,
        user.Email,
        null,
        roles,
        roles.Contains(Domain.Accounts.Roles.Provider));
}
