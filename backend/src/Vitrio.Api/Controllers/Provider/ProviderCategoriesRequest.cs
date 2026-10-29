using Vitrio.Application.Providers.SetProviderCategories;

namespace Vitrio.Api.Controllers.Provider;

public sealed record ProviderCategoriesRequest(IReadOnlyList<int>? CategoryIds, int? PrimaryCategoryId)
{
    public SetProviderCategoriesCommand ToCommand() => new(CategoryIds ?? [], PrimaryCategoryId ?? 0);
}
