using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Locations.SearchCities;

public sealed class SearchCitiesQueryHandler(ICitySearchableRepository cities, IReadCache cache)
    : IQueryHandler<SearchCitiesQuery, Result<IReadOnlyList<CitySummary>>>
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);
    private static readonly TimeSpan LocalLifetime = TimeSpan.FromHours(1);

    public async ValueTask<Result<IReadOnlyList<CitySummary>>> Handle(SearchCitiesQuery query, CancellationToken cancellationToken)
    {
        var term = SearchText.Normalize(query.Search);
        var limit = query.Limit ?? SearchCitiesQuery.DefaultLimit;
        var found = await cache.GetOrCreateAsync(
            $"cities:search:{query.State}:{limit}:{term}",
            token => new ValueTask<IReadOnlyList<CitySummary>>(cities.SearchAsync(term, query.State, limit, token)),
            Lifetime,
            LocalLifetime,
            [CacheTags.Cities],
            cancellationToken);
        return Result<IReadOnlyList<CitySummary>>.Success(found);
    }
}
