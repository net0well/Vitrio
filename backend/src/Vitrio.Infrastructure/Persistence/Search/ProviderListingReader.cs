using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Storage;

namespace Vitrio.Infrastructure.Persistence.Search;

internal sealed class ProviderListingReader(AppDbContext db, PublicMedia media)
{
    private const decimal CentsPerReal = 100;

    public async Task<List<ProviderListingRow>> RowsAsync(ProviderListingStatement statement, CancellationToken cancellationToken) =>
        await db.Database
            .SqlQueryRaw<ProviderListingRow>(statement.Sql, [.. statement.Parameters])
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProviderSearchItem>> ItemsAsync(ProviderListingStatement statement, CancellationToken cancellationToken) =>
        (await RowsAsync(statement, cancellationToken)).Select(ToItem).ToList();

    public async Task<bool> AnyAsync(ProviderListingStatement statement, CancellationToken cancellationToken) =>
        (await db.Database
            .SqlQueryRaw<ProviderMatchRow>(statement.Sql, [.. statement.Parameters])
            .SingleAsync(cancellationToken)).Found;

    public ProviderSearchItem ToItem(ProviderListingRow row)
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
