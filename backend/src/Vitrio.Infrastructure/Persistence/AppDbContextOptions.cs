using Microsoft.EntityFrameworkCore;

namespace Vitrio.Infrastructure.Persistence;

public static class AppDbContextOptions
{
    public const string ConnectionStringName = "Database";

    public static DbContextOptionsBuilder UseVitrioDatabase(this DbContextOptionsBuilder options, string connectionString) =>
        options
            .UseNpgsql(connectionString, npgsql => npgsql
                .UseNetTopologySuite(geographyAsDefault: true)
                .MigrationsHistoryTable("__ef_migrations_history"))
            .UseSnakeCaseNamingConvention();
}
