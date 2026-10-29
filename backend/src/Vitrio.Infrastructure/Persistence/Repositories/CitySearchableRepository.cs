using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Infrastructure.Persistence.Cities;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class CitySearchableRepository(AppDbContext db) : ICitySearchableRepository
{
    public async Task<LocateResponse?> LocateAsync(
        double latitude,
        double longitude,
        double nearestWithinMeters,
        CancellationToken cancellationToken)
    {
        var point = new Point(longitude, latitude) { SRID = GeoJsonBoundary.Wgs84 };
        var inside = await db.Cities
            .AsNoTracking()
            .Where(city => city.Boundary.Covers(point))
            .Select(city => new CitySummary(city.IbgeCode, city.Name, city.State))
            .FirstOrDefaultAsync(cancellationToken);
        if (inside is not null)
        {
            return new LocateResponse(inside, LocatePrecision.Boundary);
        }

        var nearest = await db.Cities
            .AsNoTracking()
            .Where(city => city.Centroid.IsWithinDistance(point, nearestWithinMeters))
            .OrderBy(city => city.Centroid.Distance(point))
            .Select(city => new CitySummary(city.IbgeCode, city.Name, city.State))
            .FirstOrDefaultAsync(cancellationToken);
        return nearest is null ? null : new LocateResponse(nearest, LocatePrecision.Nearest);
    }

    public async Task<IReadOnlyList<CitySummary>> SearchAsync(
        string normalizedTerm,
        string? state,
        int limit,
        CancellationToken cancellationToken) =>
        await db.Cities
            .AsNoTracking()
            .Where(city => city.NameNormalized.Contains(normalizedTerm) && (state == null || city.State == state))
            .OrderBy(city => city.NameNormalized.StartsWith(normalizedTerm) ? 0 : 1)
            .ThenBy(city => city.NameNormalized.Length)
            .ThenBy(city => city.Name)
            .Take(limit)
            .Select(city => new CitySummary(city.IbgeCode, city.Name, city.State))
            .ToListAsync(cancellationToken);

    public async Task<CityDetails?> GetAsync(int ibgeCode, CancellationToken cancellationToken)
    {
        var city = await db.Cities
            .AsNoTracking()
            .Where(record => record.IbgeCode == ibgeCode)
            .Select(record => new { record.IbgeCode, record.Name, record.State, record.Centroid, record.TimeZone })
            .SingleOrDefaultAsync(cancellationToken);
        return city is null
            ? null
            : new CityDetails(city.IbgeCode, city.Name, city.State, new GeoPoint(city.Centroid.Y, city.Centroid.X), city.TimeZone);
    }

    public Task<CitySummary?> FindAsync(int ibgeCode, CancellationToken cancellationToken) =>
        db.Cities
            .AsNoTracking()
            .Where(city => city.IbgeCode == ibgeCode)
            .Select(city => new CitySummary(city.IbgeCode, city.Name, city.State))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<CitySummary?> FindByNameAsync(string state, string normalizedName, CancellationToken cancellationToken) =>
        db.Cities
            .AsNoTracking()
            .Where(city => city.State == state && city.NameNormalized == normalizedName)
            .Select(city => new CitySummary(city.IbgeCode, city.Name, city.State))
            .FirstOrDefaultAsync(cancellationToken);
}
