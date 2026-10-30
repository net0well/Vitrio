namespace Vitrio.Domain.Locations;

public sealed record Coordinates(double Latitude, double Longitude)
{
    private const double CellDegrees = 0.0045;
    private const int StoredDecimals = 6;

    public static Coordinates? InBrazil(double? latitude, double? longitude) =>
        latitude is { } lat && longitude is { } lng && BrazilBounds.Contains(lat, lng) ? new Coordinates(lat, lng) : null;

    public Coordinates ToPublicCell()
    {
        var latitude = CellCenter(Latitude, CellDegrees);
        var longitudeStep = CellDegrees / Math.Cos(latitude * Math.PI / 180);
        return new Coordinates(
            Math.Round(latitude, StoredDecimals),
            Math.Round(CellCenter(Longitude, longitudeStep), StoredDecimals));
    }

    private static double CellCenter(double value, double step) => (Math.Floor(value / step) + 0.5) * step;
}
