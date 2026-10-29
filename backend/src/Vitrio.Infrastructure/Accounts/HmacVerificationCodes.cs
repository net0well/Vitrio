using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Accounts;

internal sealed partial class HmacVerificationCodes : IVerificationCodes
{
    public const int MinKeyBytes = 32;

    private readonly byte[] _key;

    public HmacVerificationCodes(IOptions<AuthOptions> options, ILogger<HmacVerificationCodes> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Value.CodeKey))
        {
            _key = RandomNumberGenerator.GetBytes(MinKeyBytes);
            LogTemporaryKey(logger);
            return;
        }

        _key = Convert.FromBase64String(options.Value.CodeKey);
        if (_key.Length < MinKeyBytes)
        {
            throw new InvalidOperationException($"Auth:CodeKey must have at least {MinKeyBytes} bytes in base64.");
        }
    }

    public string Generate() => RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);

    public string Hash(EmailAddress email, EmailCodePurpose purpose, string code) =>
        Convert.ToHexStringLower(HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes($"{purpose}\n{email.Value}\n{code}")));

    [LoggerMessage(Level = LogLevel.Warning, Message = "Auth:CodeKey is not configured; using a temporary key, so e-mail codes stop working when the API restarts")]
    private static partial void LogTemporaryKey(ILogger logger);
}
