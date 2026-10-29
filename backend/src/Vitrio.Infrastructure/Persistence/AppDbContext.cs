using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Auditing;
using Vitrio.Infrastructure.Persistence.Cities;
using Vitrio.Infrastructure.Persistence.Outbox;
using Vitrio.Infrastructure.Persistence.Protection;

namespace Vitrio.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<User> Users => Set<User>();

    public DbSet<EmailCode> EmailCodes => Set<EmailCode>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Address> Addresses => Set<Address>();

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<AdminGrant> AdminGrants => Set<AdminGrant>();

    public DbSet<CityRecord> Cities => Set<CityRecord>();

    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

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
        PersonalDataEncryption.Apply(modelBuilder, this.GetService<IDbContextOptions>());
    }
}
