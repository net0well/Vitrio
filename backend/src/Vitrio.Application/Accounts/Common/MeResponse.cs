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
    public static MeResponse From(User user, IReadOnlyCollection<string> roles) =>
        Create(user.Id, user.DisplayName, user.Email, user.PhoneNumber, roles);

    public static MeResponse Create(Guid id, string displayName, string email, string? phoneNumber, IReadOnlyCollection<string> roles) => new(
        id,
        displayName,
        Domain.Accounts.DisplayName.FromStored(displayName).Initials,
        email,
        phoneNumber,
        roles,
        roles.Contains(Domain.Accounts.Roles.Provider));
}
