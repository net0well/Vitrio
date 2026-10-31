using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Search.Services;

public interface ISearchProfessionalService
{
    Task<ProfessionalNameResponse?> FindAsync(int? categoryId, string? term, CancellationToken cancellationToken);
}
