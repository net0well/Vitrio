using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class EmailCodeWritableRepository(AppDbContext db) : IEmailCodeWritableRepository
{
    public async Task<IReadOnlyList<EmailCode>> ListIssuedSinceAsync(
        EmailAddress email,
        DateTimeOffset since,
        CancellationToken cancellationToken) =>
        await db.EmailCodes
            .Where(code => code.Email == email.Value && code.CreatedAt > since)
            .OrderBy(code => code.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<EmailCode?> GetLatestAsync(EmailAddress email, EmailCodePurpose purpose, CancellationToken cancellationToken) =>
        db.EmailCodes
            .Where(code => code.Email == email.Value && code.Purpose == purpose)
            .OrderByDescending(code => code.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public void Add(EmailCode code) => db.EmailCodes.Add(code);
}
