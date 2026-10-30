using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Links;

internal static class LinksRegistration
{
    public static IServiceCollection AddPublicLinks(this IServiceCollection services)
    {
        services.AddOptions<PublicLinksOptions>()
            .BindConfiguration(PublicLinksOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IPublicLinks, PublicLinks>();
        return services;
    }
}
