using Mediator;
using Vitrio.Application.Search.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Search.GetHome;

public sealed record GetHomeQuery(double? Latitude, double? Longitude, int? CityIbgeCode) : IQuery<Result<HomeResponse>>, ISearchLocation
{
    public override string ToString() => nameof(GetHomeQuery);
}
