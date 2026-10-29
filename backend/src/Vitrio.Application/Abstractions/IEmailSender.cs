using Vitrio.Application.Accounts.Emails;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Abstractions;

public interface IEmailSender
{
    Task<Result> SendAsync(AccountEmail email, CancellationToken cancellationToken);
}
