using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Common;

public sealed class SessionStarter(
    IAppDbContext db,
    IRefreshTokenFactory refreshTokens,
    ITokenIssuer tokenIssuer,
    IClock clock)
{
    public AuthResponse Start(User user, string? deviceName)
    {
        var session = Session.Start(user.Id, deviceName, clock);
        var refreshToken = refreshTokens.Create();
        db.Sessions.Add(session);
        db.RefreshTokens.Add(RefreshToken.IssueFor(session, refreshToken.Hash, clock));

        IReadOnlyCollection<string> roles = [Roles.Customer];
        var accessToken = tokenIssuer.Issue(user, session, roles);
        return new AuthResponse(accessToken.Value, accessToken.ExpiresInSeconds, refreshToken.Value, MeResponse.From(user, roles));
    }
}
