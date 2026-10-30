using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Search.Repositories;

public interface IProviderListingSearchableRepository
{
    Task<IReadOnlyList<ProviderSearchItem>> SearchAsync(ProviderListingCriteria criteria, CancellationToken cancellationToken);
}
