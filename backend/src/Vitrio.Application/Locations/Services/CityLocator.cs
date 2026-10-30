using System.Globalization;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;

namespace Vitrio.Application.Locations.Services;

internal sealed class CityLocator(ICitySearchableRepository cities, IReadCache cache) : ICityLocator
{
    private const double NearestWithinMeters = 30_000;

    private static readonly TimeSpan CellLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan LocalCellLifetime = TimeSpan.FromMinutes(1);

    public async Task<LocateResponse?> LocateAsync(double latitude, double longitude, CancellationToken cancellationToken)
    {
        var cellLatitude = Math.Round(latitude, 2);
        var cellLongitude = Math.Round(longitude, 2);
        var key = string.Create(CultureInfo.InvariantCulture, $"cities:locate:{cellLatitude:F2}:{cellLongitude:F2}");

        return await cache.GetOrCreateAsync(
            key,
            token => new ValueTask<LocateResponse?>(cities.LocateAsync(cellLatitude, cellLongitude, NearestWithinMeters, token)),
            CellLifetime,
            LocalCellLifetime,
            [CacheTags.Cities],
            cancellationToken);
    }
}
