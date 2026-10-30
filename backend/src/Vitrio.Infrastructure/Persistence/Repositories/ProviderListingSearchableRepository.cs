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
    private const decimal CentsPerReal = 100;

    public async Task<IReadOnlyList<ProviderSearchItem>> SearchAsync(ProviderListingCriteria criteria, CancellationToken cancellationToken) =>
        (await RowsAsync(ProviderListingSql.Search(criteria, relevance.Value), cancellationToken)).Select(ToItem).ToList();

    public async Task<IReadOnlyList<TopRatedCandidate>> ListTopRatedAsync(
        SearchArea area,
        double marginMeters,
        int take,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await RowsAsync(ProviderListingSql.TopRated(area, marginMeters, take, now, relevance.Value), cancellationToken);
        return rows
            .Select(row => new TopRatedCandidate(
                ToItem(row) with { DistanceKm = null },
                row.PublicLatitude!.Value,
                row.PublicLongitude!.Value,
                row.ServiceRadiusMeters,
                row.AvailableTodaySwitch,
                row.AvailableTodaySetAt,
                row.TimeZone))
            .ToList();
    }

    public async Task<bool> HasAnyServingAsync(SearchArea area, CancellationToken cancellationToken)
    {
        var statement = ProviderListingSql.Exists(area);
        var match = await db.Database
            .SqlQueryRaw<ProviderMatchRow>(statement.Sql, [.. statement.Parameters])
            .SingleAsync(cancellationToken);
        return match.Found;
    }

    private async Task<List<ProviderListingRow>> RowsAsync(ProviderListingStatement statement, CancellationToken cancellationToken) =>
        await db.Database
            .SqlQueryRaw<ProviderListingRow>(statement.Sql, [.. statement.Parameters])
            .ToListAsync(cancellationToken);

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
            row.DistanceMeters is { } meters ? GeoDistance.Rounded(meters / SearchDistances.MetersPerKm) : null,
            new ProviderBadgesResponse(row.MeiVerified, row.AvailableToday),
            row.ResponseTimeBucket,
            row.PriceFromCents is { } cents ? new ProviderPriceResponse(cents / CentsPerReal, Enum.Parse<PriceUnit>(row.PriceUnit!)) : null);
    }
}
