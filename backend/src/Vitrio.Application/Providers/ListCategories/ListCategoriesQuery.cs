using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.ListCategories;

public sealed record ListCategoriesQuery : IQuery<Result<IReadOnlyList<CategoryResponse>>>;
