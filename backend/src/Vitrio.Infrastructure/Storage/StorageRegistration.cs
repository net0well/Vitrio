using Microsoft.Extensions.DependencyInjection;
using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.Images;

namespace Vitrio.Infrastructure.Storage;

internal static class StorageRegistration
{
    public static IServiceCollection AddStorage(this IServiceCollection services)
    {
        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<StorageClients>();
        services.AddSingleton<PublicMedia>();
        services.AddSingleton<IObjectStorage, S3ObjectStorage>();
        services.AddSingleton<IImageProcessor, SkiaImageProcessor>();
        services.AddHostedService<StorageBucketsInitializer>();
        return services;
    }
}
