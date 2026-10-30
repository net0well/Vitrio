using Vitrio.Application.Search.Common;

namespace Vitrio.Application.Search.Repositories;

public interface IProviderListingSearchableRepository
{
    Task<IReadOnlyList<ProviderSearchItem>> SearchAsync(ProviderListingCriteria criteria, CancellationToken cancellationToken);

    Task<IReadOnlyList<TopRatedCandidate>> ListTopRatedAsync(
        SearchArea area,
        double marginMeters,
        int take,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<bool> HasAnyServingAsync(SearchArea area, CancellationToken cancellationToken);
}
