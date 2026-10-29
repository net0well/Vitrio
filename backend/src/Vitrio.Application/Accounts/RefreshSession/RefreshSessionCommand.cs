using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.RefreshSession;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<Result<TokenResponse>>
{
    public override string ToString() => nameof(RefreshSessionCommand);
}
