using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ChangePassword;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword) : ICommand<Result>
{
    public override string ToString() => nameof(ChangePasswordCommand);
}
