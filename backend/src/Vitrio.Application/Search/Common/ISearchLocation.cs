namespace Vitrio.Application.Search.Common;

public interface ISearchLocation
{
    double? Latitude { get; }

    double? Longitude { get; }

    int? CityIbgeCode { get; }
}
