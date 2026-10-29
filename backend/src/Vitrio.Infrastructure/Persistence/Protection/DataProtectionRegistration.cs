using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Vitrio.Infrastructure.Persistence.Protection;

internal static class DataProtectionRegistration
{
    public static IServiceCollection AddPersonalDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(DataProtectionSettings.SectionName).Get<DataProtectionSettings>() ?? new DataProtectionSettings();
        var builder = services.AddDataProtection()
            .SetApplicationName("vitrio")
            .PersistKeysToDbContext<AppDbContext>();

        if (!string.IsNullOrWhiteSpace(settings.Certificate))
        {
            var certificate = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(settings.Certificate), settings.CertificatePassword);
            builder.ProtectKeysWithCertificate(certificate).UnprotectKeysWithAnyCertificate(certificate);
        }

        services.AddSingleton<IPersonalDataProtector, PersonalDataProtector>();
        return services;
    }
}
