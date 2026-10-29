namespace Vitrio.Application.Abstractions;

public sealed record CepLookupResult(
    string? Street,
    string? Neighborhood,
    string City,
    string State,
    int? CityIbgeCode,
    double? Latitude,
    double? Longitude);
