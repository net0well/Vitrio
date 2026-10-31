using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Services;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListCategories;

public sealed class ListCategoriesQueryHandler(ICategoryListService categories)
    : IQueryHandler<ListCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<CategoryResponse>>> Handle(ListCategoriesQuery query, CancellationToken cancellationToken) =>
        Result<IReadOnlyList<CategoryResponse>>.Success(await categories.ListActiveAsync(cancellationToken));
}
