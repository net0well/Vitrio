using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions options) : DbContext(options), IAppDbContext
{
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

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
