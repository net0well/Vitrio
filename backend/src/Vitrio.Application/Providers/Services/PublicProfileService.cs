using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;

namespace Vitrio.Application.Providers.Services;

internal sealed class PublicProfileService(IProviderSearchableRepository providers, IReadCache cache) : IPublicProfileService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LocalLifetime = TimeSpan.FromSeconds(30);

    public async Task<ProviderPublicSnapshot?> GetSnapshotAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var tag = ProviderCacheTags.Provider(providerId);
        return await cache.GetOrCreateAsync(
            tag,
            token => new ValueTask<ProviderPublicSnapshot?>(providers.GetPublicSnapshotAsync(providerId, token)),
            Lifetime,
            LocalLifetime,
            [tag],
            cancellationToken);
    }
}
