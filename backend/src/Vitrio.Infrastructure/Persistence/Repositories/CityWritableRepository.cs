using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Infrastructure.Persistence.Cities;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class CityWritableRepository(AppDbContext db) : ICityWritableRepository
{
    public async Task UpsertAsync(IReadOnlyList<CityImport> cities, CancellationToken cancellationToken)
    {
        var codes = cities.Select(city => city.IbgeCode).ToList();
        var existing = await db.Cities
            .Where(city => codes.Contains(city.IbgeCode))
            .ToDictionaryAsync(city => city.IbgeCode, cancellationToken);

        foreach (var city in cities)
        {
            if (!existing.TryGetValue(city.IbgeCode, out var record))
            {
                record = CityRecord.Create(city.IbgeCode);
                db.Cities.Add(record);
            }

            record.Update(city.Name, city.NameNormalized, city.State, city.TimeZone, GeoJsonBoundary.Parse(city.BoundaryGeoJson));
        }
    }
}
