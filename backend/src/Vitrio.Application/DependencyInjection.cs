using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Accounts.Services;

namespace Vitrio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<EmailCodeService>();
        services.AddScoped<SessionService>();
        services.AddScoped<PasswordService>();
        return services;
    }
}
