using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.VerifyEmail;

public sealed record VerifyEmailCommand(string Email, string Code, string Password, string? DeviceName)
    : ICommand<Result<AuthResponse>>
{
    public override string ToString() => nameof(VerifyEmailCommand);
}
