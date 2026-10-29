using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Accounts.Services;

public sealed class PasswordService(IPasswordHasher hasher)
{
    private readonly Lazy<string> _unusedHash = new(() => hasher.Hash(Guid.NewGuid().ToString()));

    public string Hash(string password) => hasher.Hash(PasswordPolicy.Normalize(password));

    public bool Matches(string passwordHash, string password) =>
        hasher.Verify(passwordHash, PasswordPolicy.Normalize(password)) != PasswordCheck.Failed;

    public void MatchWithoutAccount(string password) => Matches(_unusedHash.Value, password);
}
