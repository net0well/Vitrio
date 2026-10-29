using Microsoft.EntityFrameworkCore;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Auditing;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<EmailCode> EmailCodes => Set<EmailCode>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<OutboxDelivery> OutboxDeliveries => Set<OutboxDelivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("postgis")
            .HasPostgresExtension("unaccent")
            .HasPostgresExtension("pg_trgm")
            .HasPostgresExtension("citext");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
