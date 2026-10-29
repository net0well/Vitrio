using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListCategories;

public sealed class ListCategoriesQueryHandler(ICategorySearchableRepository categories, IReadCache cache)
    : IQueryHandler<ListCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan LocalLifetime = TimeSpan.FromMinutes(5);

    public async ValueTask<Result<IReadOnlyList<CategoryResponse>>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken)
    {
        var found = await cache.GetOrCreateAsync(
            ProviderCacheTags.Categories,
            token => new ValueTask<IReadOnlyList<CategoryResponse>>(categories.ListActiveAsync(token)),
            Lifetime,
            LocalLifetime,
            [ProviderCacheTags.Categories],
            cancellationToken);
        return Result<IReadOnlyList<CategoryResponse>>.Success(found);
    }
}
