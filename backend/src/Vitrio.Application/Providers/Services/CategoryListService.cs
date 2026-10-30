using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;

namespace Vitrio.Application.Providers.Services;

internal sealed class CategoryListService(ICategorySearchableRepository categories, IReadCache cache) : ICategoryListService
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan LocalLifetime = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            ProviderCacheTags.Categories,
            token => new ValueTask<IReadOnlyList<CategoryResponse>>(categories.ListActiveAsync(token)),
            Lifetime,
            LocalLifetime,
            [ProviderCacheTags.Categories],
            cancellationToken);
}
