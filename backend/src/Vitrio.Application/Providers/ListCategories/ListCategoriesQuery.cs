using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListCategories;

public sealed record ListCategoriesQuery : IQuery<Result<IReadOnlyList<CategoryResponse>>>;
