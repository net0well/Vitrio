namespace Vitrio.Application.Abstractions;

public sealed record CepLookupResult(
    string? Street,
    string? Neighborhood,
    string City,
    string State,
    double? Latitude,
    double? Longitude);
