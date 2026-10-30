using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.Services;

internal sealed class AddressLookupService(ICepLookup cepLookup, ICitySearchableRepository cities) : IAddressLookupService
{
    public async Task<Result<CepDetails?>> LookupAsync(Cep cep, CancellationToken cancellationToken)
    {
        var lookup = await cepLookup.FindAsync(cep, cancellationToken);
        if (lookup.IsFailure)
        {
            return lookup.Error;
        }

        if (lookup.Value is not { } found)
        {
            return Result<CepDetails?>.Success(null);
        }

        if (!BrazilStates.IsState(found.State))
        {
            return AccountErrors.CityNotFound;
        }

        var city = await FindCityAsync(found, cancellationToken);
        return city is null
            ? AccountErrors.CityNotFound
            : new CepDetails(found.Street, found.Neighborhood, city, found.Latitude, found.Longitude);
    }

    public async Task<Result<PostalLocation>> ResolveAsync(Cep cep, ManualAddress manual, CancellationToken cancellationToken)
    {
        var lookup = await LookupAsync(cep, cancellationToken);
        if (lookup.IsSuccess && lookup.Value is { } found)
        {
            return PostalLocation.Create(
                string.IsNullOrWhiteSpace(found.Street) ? manual.Street : found.Street,
                string.IsNullOrWhiteSpace(found.Neighborhood) ? manual.Neighborhood : found.Neighborhood,
                found.City.Name,
                found.City.State,
                found.City.IbgeCode,
                found.Latitude,
                found.Longitude);
        }

        if (manual.IsEmpty)
        {
            return lookup.IsFailure ? lookup.Error : AccountErrors.CepNotFound;
        }

        return await FromManualAsync(manual, cancellationToken);
    }

    private async Task<CitySummary?> FindCityAsync(CepLookupResult found, CancellationToken cancellationToken)
    {
        if (found.CityIbgeCode is { } code && await cities.FindAsync(code, cancellationToken) is { } byCode && byCode.State == found.State)
        {
            return byCode;
        }

        return await cities.FindByNameAsync(found.State, SearchText.Normalize(found.City), cancellationToken);
    }

    private async Task<Result<PostalLocation>> FromManualAsync(ManualAddress manual, CancellationToken cancellationToken)
    {
        if (!BrazilStates.IsState(manual.State) || manual.CityIbgeCode is not { } code)
        {
            return AccountErrors.CityNotFound;
        }

        var city = await cities.FindAsync(code, cancellationToken);
        return city is null || city.State != manual.State
            ? AccountErrors.CityNotFound
            : PostalLocation.Create(manual.Street, manual.Neighborhood, city.Name, city.State, city.IbgeCode, null, null);
    }
}
