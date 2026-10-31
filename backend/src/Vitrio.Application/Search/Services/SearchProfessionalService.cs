using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Services;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Search.Services;

internal sealed class SearchProfessionalService(ICategoryListService categories) : ISearchProfessionalService
{
    public async Task<ProfessionalNameResponse?> FindAsync(int? categoryId, string? term, CancellationToken cancellationToken)
    {
        if (categoryId is null && term is null)
        {
            return null;
        }

        var active = await categories.ListActiveAsync(cancellationToken);
        var category = categoryId is { } id
            ? active.FirstOrDefault(category => category.Id == id)
            : active.FirstOrDefault(category => IsNamed(category, term!));
        return category is null ? null : new ProfessionalNameResponse(category.ProfessionalSingular, category.ProfessionalPlural);
    }

    private static bool IsNamed(CategoryResponse category, string term) =>
        SearchText.Normalize(category.ProfessionalSingular) == term || SearchText.Normalize(category.ProfessionalPlural) == term;
}
