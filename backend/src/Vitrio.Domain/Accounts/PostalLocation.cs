using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;

namespace Vitrio.Domain.Accounts;

public sealed record PostalLocation
{
    public const int StreetMaxLength = 120;
    public const int NeighborhoodMaxLength = 80;
    public const int CityMaxLength = 80;

    private PostalLocation(string street, string neighborhood, string city, string state, int cityIbgeCode, double? latitude, double? longitude)
    {
        Street = street;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        CityIbgeCode = cityIbgeCode;
        Latitude = latitude;
        Longitude = longitude;
    }

    public string Street { get; }

    public string Neighborhood { get; }

    public string City { get; }

    public string State { get; }

    public int CityIbgeCode { get; }

    public double? Latitude { get; }

    public double? Longitude { get; }

    public static bool IsState(string? state) => BrazilStates.IsState(state);

    public static Result<PostalLocation> Create(
        string? street,
        string? neighborhood,
        string? city,
        string? state,
        int cityIbgeCode,
        double? latitude,
        double? longitude)
    {
        var normalizedStreet = FreeText.Normalize(street);
        var normalizedNeighborhood = FreeText.Normalize(neighborhood);
        var normalizedCity = FreeText.Normalize(city);
        if (normalizedStreet.Length is 0 or > StreetMaxLength)
        {
            return AccountErrors.StreetRequired;
        }

        if (normalizedNeighborhood.Length is 0 or > NeighborhoodMaxLength)
        {
            return AccountErrors.NeighborhoodRequired;
        }

        if (normalizedCity.Length is 0 or > CityMaxLength || !IsState(state) || cityIbgeCode is < 1_000_000 or > 9_999_999)
        {
            return AccountErrors.CityNotFound;
        }

        var hasCoordinates = latitude is >= -90 and <= 90 && longitude is >= -180 and <= 180;
        return new PostalLocation(
            normalizedStreet,
            normalizedNeighborhood,
            normalizedCity,
            state!,
            cityIbgeCode,
            hasCoordinates ? latitude : null,
            hasCoordinates ? longitude : null);
    }
}
