using Vitrio.Application.Locations.Common;

namespace Vitrio.Application.Locations.Repositories;

public interface ICityWritableRepository
{
    Task UpsertAsync(IReadOnlyList<CityImport> cities, CancellationToken cancellationToken);
}
