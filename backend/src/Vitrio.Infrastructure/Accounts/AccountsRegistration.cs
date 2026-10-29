using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.Authentication;

namespace Vitrio.Infrastructure.Accounts;

internal static class AccountsRegistration
{
    public static IServiceCollection AddAccounts(this IServiceCollection services)
    {
        services.AddOptions<AuthOptions>().BindConfiguration(AuthOptions.SectionName);
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<JwtSigningKey>();
        services.AddSingleton<ITokenIssuer, JwtTokenIssuer>();
        services.AddSingleton<IVerificationCodes, HmacVerificationCodes>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();
        services.AddSingleton<Application.Abstractions.IPasswordHasher, IdentityPasswordHasher>();
        services.AddSingleton<ISessionStatusCache, SessionStatusCache>();
        services.AddScoped<SessionStatusChecker>();
        return services;
    }
}
