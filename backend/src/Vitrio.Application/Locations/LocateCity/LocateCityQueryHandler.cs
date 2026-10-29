using System.Globalization;
using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Domain.Locations;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Locations.LocateCity;

public sealed class LocateCityQueryHandler(ICitySearchableRepository cities, IReadCache cache)
    : IQueryHandler<LocateCityQuery, Result<LocateResponse>>
{
    public const double NearestWithinMeters = 30_000;

    private static readonly TimeSpan CellLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LocalCellLifetime = TimeSpan.FromMinutes(1);

    public async ValueTask<Result<LocateResponse>> Handle(LocateCityQuery query, CancellationToken cancellationToken)
    {
        var latitude = Math.Round(query.Latitude!.Value, 2);
        var longitude = Math.Round(query.Longitude!.Value, 2);
        var key = string.Create(CultureInfo.InvariantCulture, $"cities:locate:{latitude:F2}:{longitude:F2}");

        var found = await cache.GetOrCreateAsync(
            key,
            token => new ValueTask<LocateResponse?>(cities.LocateAsync(latitude, longitude, NearestWithinMeters, token)),
            CellLifetime,
            LocalCellLifetime,
            [CacheTags.Cities],
            cancellationToken);

        return found is null ? LocationErrors.NoCityNearby : found;
    }
}
