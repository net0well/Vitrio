using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.Register;

public sealed record RegisterCommand(string DisplayName, string Email, string Password, string AcceptedTermsVersion)
    : ICommand<Result>
{
    public override string ToString() => nameof(RegisterCommand);
}
