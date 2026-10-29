using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand<Result>
{
    public override string ToString() => nameof(ChangePasswordCommand);
}
