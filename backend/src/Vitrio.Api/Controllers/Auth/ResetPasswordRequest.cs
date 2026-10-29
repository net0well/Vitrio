using Vitrio.Application.Accounts.ResetPassword;

namespace Vitrio.Api.Controllers.Auth;

public sealed record ResetPasswordRequest(string? Email, string? Code, string? NewPassword, string? DeviceName)
{
    public ResetPasswordCommand ToCommand() =>
        new(Email ?? string.Empty, Code ?? string.Empty, NewPassword ?? string.Empty, DeviceName);

    public override string ToString() => nameof(ResetPasswordRequest);
}
