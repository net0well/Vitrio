using Vitrio.Application.Accounts.ForgotPassword;

namespace Vitrio.Api.Controllers.Auth;

public sealed record ForgotPasswordRequest(string? Email)
{
    public ForgotPasswordCommand ToCommand() => new(Email ?? string.Empty);
}
