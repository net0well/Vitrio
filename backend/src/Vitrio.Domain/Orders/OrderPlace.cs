namespace Vitrio.Domain.Orders;

public sealed record OrderPlace(int CityIbgeCode, string CityName, string State, string Neighborhood, string TimeZoneId, double Latitude, double Longitude, bool IsApproximate);
