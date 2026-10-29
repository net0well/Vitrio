using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Infrastructure.Authentication;

internal sealed class JwtTokenIssuer(IOptions<JwtOptions> options, JwtSigningKey signingKey, IClock clock) : ITokenIssuer
{
    private readonly JsonWebTokenHandler _handler = new() { SetDefaultTimesOnTokenCreation = false };

    public AccessToken Issue(User user, Session session, IReadOnlyCollection<string> roles)
    {
        var jwt = options.Value;
        var now = clock.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            IssuedAt = now.UtcDateTime,
            Expires = (now + jwt.AccessTokenLifetime).UtcDateTime,
            SigningCredentials = new SigningCredentials(signingKey.Key, SecurityAlgorithms.EcdsaSha256),
            Claims = new Dictionary<string, object>
            {
                [ClaimNames.Subject] = user.Id.ToString(),
                [ClaimNames.SessionId] = session.Id.ToString(),
                [ClaimNames.Roles] = roles.ToArray(),
                [ClaimNames.TokenId] = Guid.CreateVersion7(now).ToString()
            }
        };

        return new AccessToken(_handler.CreateToken(descriptor), (int)jwt.AccessTokenLifetime.TotalSeconds);
    }
}
