using Vitrio.Application.Accounts.Login;

namespace Vitrio.Api.Controllers.Auth;

public sealed record LoginRequest(string? Email, string? Password, string? DeviceName)
{
    public LoginCommand ToCommand() => new(Email ?? string.Empty, Password ?? string.Empty, DeviceName);

    public override string ToString() => nameof(LoginRequest);
}
