using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Locations.Repositories;

public interface ICitySearchableRepository
{
    Task<LocateResponse?> LocateAsync(double latitude, double longitude, double nearestWithinMeters, CancellationToken cancellationToken);

    Task<IReadOnlyList<CitySummary>> SearchAsync(string normalizedTerm, string? state, int limit, CancellationToken cancellationToken);

    Task<CityDetails?> GetAsync(int ibgeCode, CancellationToken cancellationToken);

    Task<CitySummary?> FindAsync(int ibgeCode, CancellationToken cancellationToken);

    Task<CitySummary?> FindByNameAsync(string state, string normalizedName, CancellationToken cancellationToken);
}
