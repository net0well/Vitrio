namespace Vitrio.Domain.Locations;

public static class GeoDistance
{
    private const double EarthRadiusKm = 6371.0088;
    private const double OneDecimalUpToKm = 10;

    public static double Kilometers(Coordinates from, Coordinates to)
    {
        var deltaLatitude = Radians(to.Latitude - from.Latitude);
        var deltaLongitude = Radians(to.Longitude - from.Longitude);
        var haversine = Math.Pow(Math.Sin(deltaLatitude / 2), 2)
            + (Math.Cos(Radians(from.Latitude)) * Math.Cos(Radians(to.Latitude)) * Math.Pow(Math.Sin(deltaLongitude / 2), 2));
        return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(haversine));
    }

    public static double Rounded(double kilometers) =>
        kilometers < OneDecimalUpToKm ? Math.Round(kilometers, 1) : Math.Round(kilometers);

    private static double Radians(double degrees) => degrees * Math.PI / 180;
}
