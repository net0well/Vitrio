using Mediator;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Services;
using Vitrio.Domain.Locations;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Locations.LocateCity;

public sealed class LocateCityQueryHandler(ICityLocator locator) : IQueryHandler<LocateCityQuery, Result<LocateResponse>>
{
    public async ValueTask<Result<LocateResponse>> Handle(LocateCityQuery query, CancellationToken cancellationToken)
    {
        var found = await locator.LocateAsync(query.Latitude!.Value, query.Longitude!.Value, cancellationToken);
        return found is null ? LocationErrors.NoCityNearby : found;
    }
}
