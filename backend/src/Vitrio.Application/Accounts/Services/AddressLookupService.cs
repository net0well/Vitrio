using System.Globalization;
using System.Text;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

internal sealed class AddressLookupService(ICepLookup cepLookup, ICityDirectory cities) : IAddressLookupService
{
    public async Task<Result<PostalLocation>> ResolveAsync(Cep cep, ManualAddress manual, CancellationToken cancellationToken)
    {
        var lookup = await cepLookup.FindAsync(cep, cancellationToken);
        if (lookup.IsSuccess && lookup.Value is { } found)
        {
            return await FromLookupAsync(found, manual, cancellationToken);
        }

        if (manual.IsEmpty)
        {
            return lookup.IsFailure ? lookup.Error : AccountErrors.CepNotFound;
        }

        return await FromManualAsync(manual, cancellationToken);
    }

    private async Task<Result<PostalLocation>> FromLookupAsync(CepLookupResult found, ManualAddress manual, CancellationToken cancellationToken)
    {
        if (!PostalLocation.IsState(found.State))
        {
            return AccountErrors.CityNotFound;
        }

        var list = await cities.ListAsync(found.State, cancellationToken);
        if (list.IsFailure)
        {
            return list.Error;
        }

        var city = list.Value.FirstOrDefault(candidate => candidate.Code == found.CityIbgeCode)
            ?? list.Value.FirstOrDefault(candidate => Key(candidate.Name) == Key(found.City));
        if (city is null)
        {
            return AccountErrors.CityNotFound;
        }

        return PostalLocation.Create(
            string.IsNullOrWhiteSpace(found.Street) ? manual.Street : found.Street,
            string.IsNullOrWhiteSpace(found.Neighborhood) ? manual.Neighborhood : found.Neighborhood,
            city.Name,
            found.State,
            city.Code,
            found.Latitude,
            found.Longitude);
    }

    private async Task<Result<PostalLocation>> FromManualAsync(ManualAddress manual, CancellationToken cancellationToken)
    {
        if (!PostalLocation.IsState(manual.State) || manual.CityIbgeCode is not { } code)
        {
            return AccountErrors.CityNotFound;
        }

        var list = await cities.ListAsync(manual.State!, cancellationToken);
        if (list.IsFailure)
        {
            return list.Error;
        }

        var city = list.Value.FirstOrDefault(candidate => candidate.Code == code);
        return city is null
            ? AccountErrors.CityNotFound
            : PostalLocation.Create(manual.Street, manual.Neighborhood, city.Name, manual.State, city.Code, null, null);
    }

    private static string Key(string name)
    {
        var decomposed = name.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }
}
