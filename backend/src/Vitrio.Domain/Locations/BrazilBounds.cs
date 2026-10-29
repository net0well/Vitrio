namespace Vitrio.Domain.Locations;

public static class BrazilBounds
{
    public const double MinLatitude = -34;
    public const double MaxLatitude = 6;
    public const double MinLongitude = -74;
    public const double MaxLongitude = -28;

    public static bool Contains(double latitude, double longitude) =>
        latitude is >= MinLatitude and <= MaxLatitude && longitude is >= MinLongitude and <= MaxLongitude;
}
