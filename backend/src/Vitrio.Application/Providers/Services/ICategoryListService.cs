using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Providers.Services;

public interface ICategoryListService
{
    Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken cancellationToken);
}
