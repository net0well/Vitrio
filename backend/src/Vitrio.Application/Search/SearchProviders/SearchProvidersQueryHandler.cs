using Mediator;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.Repositories;
using Vitrio.Application.Search.Services;
using Vitrio.Domain.Common;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Search.SearchProviders;

public sealed class SearchProvidersQueryHandler(
    ISearchAreaService areas,
    ISearchProfessionalService professionals,
    IProviderListingSearchableRepository listings,
    IClock clock) : IQueryHandler<SearchProvidersQuery, Result<ProviderSearchPage>>
{
    private const int MinTermLength = 2;
    private const int CentsPerReal = 100;

    public async ValueTask<Result<ProviderSearchPage>> Handle(SearchProvidersQuery query, CancellationToken cancellationToken)
    {
        var area = await areas.ResolveAsync(query.Latitude, query.Longitude, query.CityIbgeCode, cancellationToken);
        if (area.IsFailure)
        {
            return area.Error;
        }

        if (!area.Value.IsNearMe && (query.Sort == SearchSort.Distance || query.MaxDistanceKm is not null))
        {
            return SearchErrors.NeedsLocation;
        }

        var normalized = SearchText.Normalize(query.Q);
        var term = normalized.Length >= MinTermLength ? normalized : null;
        var criteria = new ProviderListingCriteria(
            area.Value,
            term,
            query.CategoryId,
            (query.MaxDistanceKm ?? SearchDistances.DefaultMaxKm) * SearchDistances.MetersPerKm,
            query.MinRating,
            query.MeiVerified,
            query.AvailableToday,
            query.MaxPriceFrom is { } maxPrice ? decimal.ToInt64(maxPrice * CentsPerReal) : null,
            query.Sort,
            (query.Page - 1) * query.PageSize,
            query.PageSize + 1,
            clock.UtcNow);
        var items = await listings.SearchAsync(criteria, cancellationToken);
        var found = await listings.CountAsync(criteria, SearchProvidersQuery.MaxCount + 1, cancellationToken);
        var professional = await professionals.FindAsync(query.CategoryId, term, cancellationToken);

        return new ProviderSearchPage(
            items.Take(query.PageSize).ToList(),
            query.Page,
            query.PageSize,
            items.Count > query.PageSize,
            Math.Min(found, SearchProvidersQuery.MaxCount),
            found > SearchProvidersQuery.MaxCount,
            SearchContextResponse.From(area.Value, professional));
    }
}
