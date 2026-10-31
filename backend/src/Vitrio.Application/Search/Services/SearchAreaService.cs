using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Application.Locations.Services;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Locations;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Search.Services;

internal sealed class SearchAreaService(ICityLocationService locator, ICitySearchableRepository cities) : ISearchAreaService
{
    public async Task<Result<SearchArea>> ResolveAsync(double? latitude, double? longitude, int? cityIbgeCode, CancellationToken cancellationToken)
    {
        var point = latitude is { } lat && longitude is { } lng ? new Coordinates(lat, lng) : null;
        if (cityIbgeCode is not { } code)
        {
            var located = point is null ? null : await locator.LocateAsync(point.Latitude, point.Longitude, cancellationToken);
            return new SearchArea(SearchMode.NearMe, point, located?.City);
        }

        var city = await cities.FindAsync(code, cancellationToken);
        if (city is null)
        {
            return SearchErrors.CityNotFound;
        }

        if (point is null)
        {
            return new SearchArea(SearchMode.City, null, city);
        }

        var pointCity = await locator.LocateAsync(point.Latitude, point.Longitude, cancellationToken);
        return pointCity is { Precision: LocatePrecision.Boundary } && pointCity.City.IbgeCode == code
            ? new SearchArea(SearchMode.NearMe, point, city)
            : new SearchArea(SearchMode.City, null, city);
    }
}
