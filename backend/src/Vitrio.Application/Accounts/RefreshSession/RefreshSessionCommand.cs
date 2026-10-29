using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RefreshSession;

public sealed record RefreshSessionCommand(string RefreshToken) : ICommand<Result<TokenResponse>>
{
    public override string ToString() => nameof(RefreshSessionCommand);
}
