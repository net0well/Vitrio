using Mediator;
using Vitrio.Application.Providers.Services;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.Repositories;
using Vitrio.Application.Search.Services;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Search.GetHome;

public sealed class GetHomeQueryHandler(
    ISearchAreaService areas,
    ICategoryListService categories,
    ITopRatedProvidersService topRated,
    IProviderListingSearchableRepository listings) : IQueryHandler<GetHomeQuery, Result<HomeResponse>>
{
    public async ValueTask<Result<HomeResponse>> Handle(GetHomeQuery query, CancellationToken cancellationToken)
    {
        var area = await areas.ResolveAsync(query.Latitude, query.Longitude, query.CityIbgeCode, cancellationToken);
        if (area.IsFailure)
        {
            return area.Error;
        }

        var hasProviders = await listings.HasAnyServingAsync(area.Value, cancellationToken);
        return new HomeResponse(
            area.Value.City,
            await categories.ListActiveAsync(cancellationToken),
            hasProviders ? await topRated.ListAsync(area.Value, cancellationToken) : [],
            hasProviders);
    }
}
