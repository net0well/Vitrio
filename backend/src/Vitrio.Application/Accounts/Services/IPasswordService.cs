using Vitrio.Application.Abstractions;

namespace Vitrio.Application.Accounts.Services;

public interface IPasswordService
{
    string Hash(string password);

    PasswordCheck Check(string passwordHash, string password);

    bool Matches(string passwordHash, string password);

    void MatchWithoutAccount(string password);
}
