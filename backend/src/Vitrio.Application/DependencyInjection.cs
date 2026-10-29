using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Accounts.Services;

namespace Vitrio.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<IEmailCodeService, EmailCodeService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IPasswordChangeService, PasswordChangeService>();
        services.AddScoped<IAddressLookupService, AddressLookupService>();
        return services;
    }
}
