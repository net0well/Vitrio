using Vitrio.Application.Accounts.RefreshSession;

namespace Vitrio.Api.Controllers.Auth;

public sealed record RefreshRequest(string? RefreshToken)
{
    public RefreshSessionCommand ToCommand() => new(RefreshToken ?? string.Empty);

    public override string ToString() => nameof(RefreshRequest);
}
