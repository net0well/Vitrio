using System.Text.Json;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Vitrio.Infrastructure.Persistence.Cities;

internal static class GeoJsonBoundary
{
    public const int Wgs84 = 4326;

    private static readonly GeometryFactory Factory = NtsGeometryServices.Instance.CreateGeometryFactory(Wgs84);

    public static MultiPolygon Parse(string geoJson)
    {
        using var document = JsonDocument.Parse(geoJson);
        var root = document.RootElement;
        var coordinates = root.GetProperty("coordinates");
        Geometry geometry = root.GetProperty("type").GetString() switch
        {
            "Polygon" => ToPolygon(coordinates),
            "MultiPolygon" => Factory.CreateMultiPolygon(coordinates.EnumerateArray().Select(ToPolygon).ToArray()),
            var other => throw new FormatException($"Unsupported boundary geometry '{other}'.")
        };

        if (!geometry.IsValid)
        {
            geometry = geometry.Buffer(0);
        }

        return geometry switch
        {
            MultiPolygon multiPolygon => multiPolygon,
            Polygon polygon => Factory.CreateMultiPolygon([polygon]),
            _ => throw new FormatException("The boundary is not a polygon.")
        };
    }

    private static Polygon ToPolygon(JsonElement rings)
    {
        var parsed = rings.EnumerateArray().Select(ToRing).ToList();
        return Factory.CreatePolygon(parsed[0], parsed.Skip(1).ToArray());
    }

    private static LinearRing ToRing(JsonElement ring) =>
        Factory.CreateLinearRing(ring.EnumerateArray().Select(point => new Coordinate(point[0].GetDouble(), point[1].GetDouble())).ToArray());
}
