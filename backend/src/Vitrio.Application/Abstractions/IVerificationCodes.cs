using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Abstractions;

public interface IVerificationCodes
{
    string Generate();

    string Hash(EmailAddress email, EmailCodePurpose purpose, string code);
}
