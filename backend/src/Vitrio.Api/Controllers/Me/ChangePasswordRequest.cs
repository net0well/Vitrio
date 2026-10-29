using Vitrio.Application.Accounts.ChangePassword;

namespace Vitrio.Api.Controllers.Me;

public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword)
{
    public ChangePasswordCommand ToCommand() => new(CurrentPassword ?? string.Empty, NewPassword ?? string.Empty);

    public override string ToString() => nameof(ChangePasswordRequest);
}
