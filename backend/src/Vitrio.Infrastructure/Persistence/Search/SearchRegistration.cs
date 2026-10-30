using Microsoft.Extensions.DependencyInjection;

namespace Vitrio.Infrastructure.Persistence.Search;

internal static class SearchRegistration
{
    public static IServiceCollection AddProviderSearch(this IServiceCollection services)
    {
        services.AddOptions<SearchRelevanceOptions>()
            .BindConfiguration(SearchRelevanceOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<ProviderSearchIndexInterceptor>();
        services.AddScoped<ProviderListingReader>();
        return services;
    }
}
