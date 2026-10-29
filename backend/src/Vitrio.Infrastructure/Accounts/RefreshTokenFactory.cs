using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Accounts;

internal sealed class RefreshTokenFactory : IRefreshTokenFactory
{
    private const int TokenBytes = 32;

    public RefreshTokenValue Create()
    {
        var value = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenBytes));
        return new RefreshTokenValue(value, Hash(value));
    }

    public string Hash(string value) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
