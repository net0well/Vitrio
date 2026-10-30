using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ResetPassword;

public sealed record ResetPasswordCommand(string Email, string Code, string NewPassword, string? DeviceName)
    : ICommand<Result<AuthResponse>>
{
    public override string ToString() => nameof(ResetPasswordCommand);
}
