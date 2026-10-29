using Microsoft.AspNetCore.Identity;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Accounts;

internal sealed class IdentityPasswordHasher : Application.Abstractions.IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public PasswordCheck Verify(string passwordHash, string password) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordCheck.Succeeded,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SucceededRehashNeeded,
            _ => PasswordCheck.Failed
        };
}
