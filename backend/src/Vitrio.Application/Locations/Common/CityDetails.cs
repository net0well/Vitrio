namespace Vitrio.Application.Locations.Common;

public sealed record CityDetails(int IbgeCode, string Name, string State, GeoPoint Centroid, string TimeZone);
