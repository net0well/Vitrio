namespace Vitrio.Application.Locations.Common;

public sealed record CityImport(int IbgeCode, string Name, string NameNormalized, string State, string TimeZone, string BoundaryGeoJson);
