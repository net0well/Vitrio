using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.Accounts;
using Vitrio.Infrastructure.Caching;
using Vitrio.Infrastructure.Email;
using Vitrio.Infrastructure.Events;
using Vitrio.Infrastructure.External;
using Vitrio.Infrastructure.Links;
using Vitrio.Infrastructure.Persistence;
using Vitrio.Infrastructure.Persistence.Outbox;
using Vitrio.Infrastructure.Persistence.Protection;
using Vitrio.Infrastructure.Persistence.Repositories;
using Vitrio.Infrastructure.Time;
using Vitrio.Shared.Time;

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
        services.AddDomainEvents(configuration);
        services.AddSingleton<OutboxInterceptor>();

        services.AddDbContext<AppDbContext>((serviceProvider, options) => options
            .UseVitrioDatabase(connectionString)
            .AddInterceptors(serviceProvider.GetRequiredService<OutboxInterceptor>()));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddRepositories();

        services.AddPersonalDataProtection(configuration);
        services.AddExternalServices();
        services.AddAccounts();
        services.AddEmail();
        services.AddPublicLinks();

        services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database", tags: [ReadinessTag]);
        services.AddVitrioCaching(configuration);

        return services;
    }
}
