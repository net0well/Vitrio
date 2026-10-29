using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<EmailCodeIssuer>();
        services.AddScoped<SessionStarter>();
        return services;
    }
}
