using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Locations.LoadCities;

public sealed class LoadCitiesCommandHandler(
    IIbgeCitySource source,
    ICityWritableRepository cities,
    IUnitOfWork unitOfWork,
    IReadCache cache) : ICommandHandler<LoadCitiesCommand, Result<int>>
{
    public async ValueTask<Result<int>> Handle(LoadCitiesCommand command, CancellationToken cancellationToken)
    {
        var loaded = 0;
        foreach (var state in command.States ?? BrazilStates.All)
        {
            var municipalities = await source.ListStateAsync(state, cancellationToken);
            await cities.UpsertAsync(municipalities.Select(ToImport).ToList(), cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            loaded += municipalities.Count;
        }

        await cache.RemoveTagAsync(CacheTags.Cities, cancellationToken);
        return loaded;
    }

    private static CityImport ToImport(IbgeMunicipality municipality) => new(
        municipality.IbgeCode,
        municipality.Name,
        SearchText.Normalize(municipality.Name),
        municipality.State,
        BrazilTimeZones.For(municipality.State, municipality.Name),
        municipality.Population,
        municipality.BoundaryGeoJson);
}
