using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vitrio.Jobs;

public static class JobsRegistration
{
    private const string ConnectionStringName = "Database";

    public static IServiceCollection AddJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(JobsOptions.SectionName).Get<JobsOptions>() ?? new JobsOptions();
        if (!options.Enabled)
        {
            return services;
        }

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{ConnectionStringName}' is not configured.");

        services.AddHangfire(hangfire => hangfire
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                storage => storage.UseNpgsqlConnection(connectionString),
                new PostgreSqlStorageOptions { SchemaName = options.Schema, PrepareSchemaIfNecessary = true }));
        services.AddHangfireServer();
        return services;
    }
}
