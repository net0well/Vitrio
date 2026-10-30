using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Locations.Services;

public interface ICityLocator
{
    Task<LocateResponse?> LocateAsync(double latitude, double longitude, CancellationToken cancellationToken);
}
