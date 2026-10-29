using Vitrio.Application.Accounts.Register;

namespace Vitrio.Api.Controllers.Auth;

public sealed record RegisterRequest(string? DisplayName, string? Email, string? Password, string? AcceptedTermsVersion)
{
    public RegisterCommand ToCommand() => new(
        DisplayName ?? string.Empty,
        Email ?? string.Empty,
        Password ?? string.Empty,
        AcceptedTermsVersion ?? string.Empty);

    public override string ToString() => nameof(RegisterRequest);
}
