using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Repositories;

public interface IEmailCodeWritableRepository
{
    Task<IReadOnlyList<EmailCode>> ListIssuedSinceAsync(EmailAddress email, DateTimeOffset since, CancellationToken cancellationToken);

    Task<EmailCode?> GetLatestAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken);

    void Add(EmailCode code);
}
