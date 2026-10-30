using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Locations.Services;

public interface ICityLocationService
{
    Task<LocateResponse?> LocateAsync(double latitude, double longitude, CancellationToken cancellationToken);
}
