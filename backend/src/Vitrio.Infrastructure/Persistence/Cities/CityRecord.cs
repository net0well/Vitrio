using NetTopologySuite.Geometries;

namespace Vitrio.Infrastructure.Persistence.Cities;

public sealed class CityRecord
{
    private CityRecord()
    {
    }

    public int IbgeCode { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string NameNormalized { get; private set; } = string.Empty;

    public string State { get; private set; } = string.Empty;

    public MultiPolygon Boundary { get; private set; } = MultiPolygon.Empty;

    public Point Centroid { get; private set; } = Point.Empty;

    public string TimeZone { get; private set; } = string.Empty;

    public static CityRecord Create(int ibgeCode) => new() { IbgeCode = ibgeCode };

    public void Update(string name, string nameNormalized, string state, string timeZone, MultiPolygon boundary)
    {
        Name = name;
        NameNormalized = nameNormalized;
        State = state;
        TimeZone = timeZone;
        Boundary = boundary;
        Centroid = (Point)boundary.InteriorPoint.Copy();
        Centroid.SRID = boundary.SRID;
    }
}
