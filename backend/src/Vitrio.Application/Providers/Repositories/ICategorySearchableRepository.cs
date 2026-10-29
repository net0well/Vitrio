using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Providers.Repositories;

public interface ICategorySearchableRepository
{
    Task<IReadOnlyList<CategoryResponse>> ListActiveAsync(CancellationToken cancellationToken);

    Task<int> CountActiveAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken);
}
