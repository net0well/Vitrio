using System.Globalization;
using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Locations.Repositories;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Locations.GetCity;

public sealed class GetCityQueryHandler(ICitySearchableRepository cities, IReadCache cache)
    : IQueryHandler<GetCityQuery, Result<CityDetails>>
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    public async ValueTask<Result<CityDetails>> Handle(GetCityQuery query, CancellationToken cancellationToken)
    {
        var found = await cache.GetOrCreateAsync(
            string.Create(CultureInfo.InvariantCulture, $"cities:get:{query.IbgeCode}"),
            token => new ValueTask<CityDetails?>(cities.GetAsync(query.IbgeCode, token)),
            Lifetime,
            [CacheTags.Cities],
            cancellationToken);
        return found is null ? LocationErrors.CityNotFound : found;
    }
}
