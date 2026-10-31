using Microsoft.Extensions.Options;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.Repositories;
using Vitrio.Infrastructure.Persistence.Search;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class ProviderListingSearchableRepository(
    ProviderListingReader reader,
    IOptions<SearchRelevanceOptions> relevance) : IProviderListingSearchableRepository
{
    public Task<IReadOnlyList<ProviderSearchItem>> SearchAsync(ProviderListingCriteria criteria, CancellationToken cancellationToken) =>
        reader.ItemsAsync(ProviderListingSql.Search(criteria, relevance.Value), cancellationToken);

    public Task<int> CountAsync(ProviderListingCriteria criteria, int limit, CancellationToken cancellationToken) =>
        reader.CountAsync(ProviderListingSql.Count(criteria, limit), cancellationToken);

    public async Task<IReadOnlyList<TopRatedCandidate>> ListTopRatedAsync(
        SearchArea area,
        double marginMeters,
        int take,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await reader.RowsAsync(ProviderListingSql.TopRated(area, marginMeters, take, now, relevance.Value), cancellationToken);
        return rows
            .Select(row => new TopRatedCandidate(
                reader.ToItem(row) with { DistanceKm = null },
                row.PublicLatitude!.Value,
                row.PublicLongitude!.Value,
                row.ServiceRadiusMeters,
                row.AvailableTodaySwitch,
                row.AvailableTodaySetAt,
                row.TimeZone))
            .ToList();
    }

    public Task<bool> HasAnyServingAsync(SearchArea area, CancellationToken cancellationToken) =>
        reader.AnyAsync(ProviderListingSql.Exists(area), cancellationToken);
}
