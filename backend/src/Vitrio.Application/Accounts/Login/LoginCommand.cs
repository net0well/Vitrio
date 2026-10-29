using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Login;

public sealed record LoginCommand(string Email, string Password, string? DeviceName) : ICommand<Result<AuthResponse>>
{
    public override string ToString() => nameof(LoginCommand);
}
