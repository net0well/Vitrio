using Mediator;
using Vitrio.Application.Locations.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Locations.GetCity;

public sealed record GetCityQuery(int IbgeCode) : IQuery<Result<CityDetails>>;
