using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Vitrio.Infrastructure.Authentication;

public sealed partial class JwtSigningKey : IDisposable
{
    private readonly ECDsa _algorithm = ECDsa.Create();

    public JwtSigningKey(IOptions<JwtOptions> options, ILogger<JwtSigningKey> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Value.SigningKey))
        {
            _algorithm.GenerateKey(ECCurve.NamedCurves.nistP256);
            LogTemporaryKey(logger);
        }
        else
        {
            _algorithm.ImportFromPem(options.Value.SigningKey);
            if (_algorithm.KeySize != 256)
            {
                throw new InvalidOperationException("Jwt:SigningKey must be an ECDSA P-256 private key.");
            }
        }

        Key = new ECDsaSecurityKey(_algorithm);
        Key.KeyId = Base64UrlEncoder.Encode(Key.ComputeJwkThumbprint());
    }

    public ECDsaSecurityKey Key { get; }

    public void Dispose() => _algorithm.Dispose();

    [LoggerMessage(Level = LogLevel.Warning, Message = "Jwt:SigningKey is not configured; using a temporary key, so tokens stop working when the API restarts")]
    private static partial void LogTemporaryKey(ILogger logger);
}
