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

        var term = SearchText.Normalize(query.Q);
        var items = await listings.SearchAsync(
            new ProviderListingCriteria(
                area.Value,
                term.Length >= MinTermLength ? term : null,
                query.CategoryId,
                (query.MaxDistanceKm ?? SearchDistances.DefaultMaxKm) * SearchDistances.MetersPerKm,
                query.MinRating,
                query.MeiVerified,
                query.AvailableToday,
                query.MaxPriceFrom is { } maxPrice ? decimal.ToInt64(maxPrice * CentsPerReal) : null,
                query.Sort,
                (query.Page - 1) * query.PageSize,
                query.PageSize + 1,
                clock.UtcNow),
            cancellationToken);

        return new ProviderSearchPage(
            items.Take(query.PageSize).ToList(),
            query.Page,
            query.PageSize,
            items.Count > query.PageSize,
            SearchContextResponse.From(area.Value));
    }
}
