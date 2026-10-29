using Mediator;
using Vitrio.Application.Locations.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Locations.LocateCity;

public sealed record LocateCityQuery(double? Latitude, double? Longitude) : IQuery<Result<LocateResponse>>
{
    public override string ToString() => nameof(LocateCityQuery);
}
