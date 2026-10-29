using Vitrio.Application.Accounts.VerifyEmail;

namespace Vitrio.Api.Controllers.Auth;

public sealed record VerifyEmailRequest(string? Email, string? Code, string? Password, string? DeviceName)
{
    public VerifyEmailCommand ToCommand() =>
        new(Email ?? string.Empty, Code ?? string.Empty, Password ?? string.Empty, DeviceName);

    public override string ToString() => nameof(VerifyEmailRequest);
}
