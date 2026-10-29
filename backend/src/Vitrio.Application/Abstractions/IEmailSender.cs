using Vitrio.Application.Accounts.Emails;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Abstractions;

public interface IEmailSender
{
    Task<Result> SendAsync(AccountEmail email, CancellationToken cancellationToken);
}
