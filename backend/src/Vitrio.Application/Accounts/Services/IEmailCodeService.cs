using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

public interface IEmailCodeService
{
    Task<Result<string>> IssueAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken);

    bool Matches(EmailCode emailCode, EmailAddress email, string candidate);
}
