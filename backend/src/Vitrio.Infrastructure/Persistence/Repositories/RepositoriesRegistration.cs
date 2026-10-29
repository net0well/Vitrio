using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Auditing.Repositories;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal static class RepositoriesRegistration
{
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserWritableRepository, UserWritableRepository>();
        services.AddScoped<IUserSearchableRepository, UserSearchableRepository>();
        services.AddScoped<IEmailCodeWritableRepository, EmailCodeWritableRepository>();
        services.AddScoped<ISessionWritableRepository, SessionWritableRepository>();
        services.AddScoped<ISessionSearchableRepository, SessionSearchableRepository>();
        services.AddScoped<IRefreshTokenWritableRepository, RefreshTokenWritableRepository>();
        services.AddScoped<IAuditEventWritableRepository, AuditEventWritableRepository>();
        return services;
    }
}
