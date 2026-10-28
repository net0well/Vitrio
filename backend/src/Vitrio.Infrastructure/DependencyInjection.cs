using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Infrastructure.Persistence.Outbox;
using Vitrio.Infrastructure.Time;

namespace Vitrio.Infrastructure;

public static class DependencyInjection
{
    public const string ReadinessTag = "ready";

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(AppDbContextOptions.ConnectionStringName)
            ?? throw new InvalidOperationException(
                $"Connection string '{AppDbContextOptions.ConnectionStringName}' is not configured.");

        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<OutboxInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseVitrioDatabase(connectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<OutboxInterceptor>()));
        services.AddScoped<IAppDbContext>(serviceProvider => serviceProvider.GetRequiredService<AppDbContext>());

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: [ReadinessTag]);

        return services;
    }
}
