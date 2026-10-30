using Vitrio.Application.Accounts.Services;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.Services;

internal sealed class ServiceAreaService(IAddressLookupService addressLookup, ICitySearchableRepository cities) : IServiceAreaService
{
    public async Task<Result<ResolvedServiceArea>> ResolveAsync(Cep cep, string number, string? complement, CancellationToken cancellationToken)
    {
        var lookup = await addressLookup.LookupAsync(cep, cancellationToken);
        if (lookup.IsFailure)
        {
            return lookup.Error;
        }

        if (lookup.Value is not { } found)
        {
            return ProviderErrors.BaseCepNotFound;
        }

        var point = Coordinates.InBrazil(found.Latitude, found.Longitude);
        if (point is null)
        {
            var city = await cities.GetAsync(found.City.IbgeCode, cancellationToken);
            if (city is null)
            {
                return AccountErrors.CityNotFound;
            }

            return Resolve(cep, number, complement, found.Street, found.Neighborhood, found.City, new Coordinates(city.Centroid.Lat, city.Centroid.Lng), true);
        }

        return Resolve(cep, number, complement, found.Street, found.Neighborhood, found.City, point, false);
    }

    private static ResolvedServiceArea Resolve(
        Cep cep,
        string number,
        string? complement,
        string? street,
        string? neighborhood,
        Locations.Common.CitySummary city,
        Coordinates point,
        bool approximate)
    {
        var normalizedComplement = FreeText.Normalize(complement);
        return new ResolvedServiceArea(
            new BaseAddress(
                cep.Value,
                street,
                FreeText.Normalize(number),
                normalizedComplement.Length == 0 ? null : normalizedComplement,
                neighborhood,
                city.IbgeCode),
            point,
            approximate,
            city);
    }
}
