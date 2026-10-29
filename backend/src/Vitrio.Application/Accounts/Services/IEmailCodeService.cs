using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.Services;

public interface IEmailCodeService
{
    Task<Result<string>> IssueAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken);

    Task<Result<AccountEmail?>> PrepareAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken);

    bool Matches(EmailCode emailCode, EmailAddress email, string candidate);
}
