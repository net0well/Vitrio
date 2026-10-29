using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Email;

internal static class EmailRegistration
{
    public static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services.AddOptions<EmailOptions>()
            .BindConfiguration(EmailOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IAccountEmailRenderer, AccountEmailRenderer>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
