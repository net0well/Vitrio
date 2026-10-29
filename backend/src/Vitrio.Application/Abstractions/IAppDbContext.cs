using Microsoft.EntityFrameworkCore;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Auditing;

namespace Vitrio.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<User> Users { get; }

    DbSet<EmailCode> EmailCodes { get; }

    DbSet<Session> Sessions { get; }

    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<AuditEvent> AuditEvents { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
