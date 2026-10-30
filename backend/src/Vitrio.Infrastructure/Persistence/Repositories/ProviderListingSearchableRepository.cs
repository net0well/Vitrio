using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence.Search;
using Vitrio.Infrastructure.Storage;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class ProviderListingSearchableRepository(
    AppDbContext db,
    PublicMedia media,
    IOptions<SearchRelevanceOptions> relevance) : IProviderListingSearchableRepository
{
    private const double MetersPerKm = 1000;
    private const decimal CentsPerReal = 100;

    public async Task<IReadOnlyList<ProviderSearchItem>> SearchAsync(ProviderListingCriteria criteria, CancellationToken cancellationToken)
    {
        var statement = ProviderListingSql.Search(criteria, relevance.Value);
        var rows = await db.Database
            .SqlQueryRaw<ProviderListingRow>(statement.Sql, [.. statement.Parameters])
            .ToListAsync(cancellationToken);
        return rows.Select(ToItem).ToList();
    }

    private ProviderSearchItem ToItem(ProviderListingRow row)
    {
        var rated = RatingRule.IsRated(row.RatingScore, row.ReviewCount);
        return new ProviderSearchItem(
            row.Id,
            row.DisplayName,
            row.Headline,
            DisplayName.FromStored(row.DisplayName).Initials,
            media.UrlOrNull(row.AvatarKey),
            rated ? new ProviderRatingResponse(row.RatingScore!.Value, row.ReviewCount) : null,
            !rated,
            row.CityName,
            row.DistanceMeters is { } meters ? GeoDistance.Rounded(meters / MetersPerKm) : null,
            new ProviderBadgesResponse(row.MeiVerified, row.AvailableToday),
            row.ResponseTimeBucket,
            row.PriceFromCents is { } cents ? new ProviderPriceResponse(cents / CentsPerReal, Enum.Parse<PriceUnit>(row.PriceUnit!)) : null);
    }
}
