using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.External.BrasilApi;
using Vitrio.Infrastructure.External.Ibge;

namespace Vitrio.Infrastructure.External;

internal static class ExternalServicesRegistration
{
    private const string UserAgent = "Vitrio/1.0 (+https://vitrio.app)";

    public static IServiceCollection AddExternalServices(this IServiceCollection services)
    {
        services.AddOptions<ExternalServicesOptions>()
            .BindConfiguration(ExternalServicesOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddHttpClient<ICepLookup, BrasilApiCepLookup>((provider, http) =>
            {
                http.BaseAddress = provider.GetRequiredService<IOptions<ExternalServicesOptions>>().Value.BrasilApiBaseUrl;
                http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            })
            .AddStandardResilienceHandler(ConfigureResilience);

        services.AddHttpClient<ICnpjLookup, BrasilApiCnpjLookup>((provider, http) =>
            {
                http.BaseAddress = provider.GetRequiredService<IOptions<ExternalServicesOptions>>().Value.BrasilApiBaseUrl;
                http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            })
            .AddStandardResilienceHandler(ConfigureResilience);

        services.AddHttpClient<IIbgeCitySource, IbgeCitySource>((provider, http) =>
            {
                http.BaseAddress = provider.GetRequiredService<IOptions<ExternalServicesOptions>>().Value.IbgeBaseUrl;
                http.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            })
            .AddStandardResilienceHandler(ConfigureBulkResilience);

        return services;
    }

    private static void ConfigureBulkResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(90);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(2);
    }

    private static void ConfigureResilience(HttpStandardResilienceOptions options)
    {
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(3);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(8);
    }
}
