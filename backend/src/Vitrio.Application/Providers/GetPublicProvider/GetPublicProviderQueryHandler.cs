using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.GetPublicProvider;

public sealed class GetPublicProviderQueryHandler(
    IProviderSearchableRepository providers,
    IReadCache cache,
    IPublicLinks links,
    IClock clock) : IQueryHandler<GetPublicProviderQuery, Result<ProviderPublicResponse>>
{
    private const int MinReviewsForRating = 3;
    private const int MinCompletedOrdersShown = 5;

    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LocalLifetime = TimeSpan.FromSeconds(30);

    public async ValueTask<Result<ProviderPublicResponse>> Handle(GetPublicProviderQuery query, CancellationToken cancellationToken)
    {
        var tag = ProviderCacheTags.Provider(query.ProviderId);
        var snapshot = await cache.GetOrCreateAsync(
            tag,
            token => new ValueTask<ProviderPublicSnapshot?>(providers.GetPublicSnapshotAsync(query.ProviderId, token)),
            Lifetime,
            LocalLifetime,
            [tag],
            cancellationToken);
        if (snapshot is null)
        {
            return ProviderErrors.NotFound;
        }

        var rated = snapshot.ReviewCount >= MinReviewsForRating && snapshot.RatingScore is not null;
        return new ProviderPublicResponse(
            snapshot.Id,
            snapshot.DisplayName,
            snapshot.Headline,
            snapshot.About,
            snapshot.YearsOfExperience,
            snapshot.AvatarUrl,
            snapshot.City,
            snapshot.Neighborhoods,
            snapshot.RadiusKm,
            snapshot.Categories,
            snapshot.Services,
            snapshot.Availability,
            snapshot.HandlesEmergencies,
            new ProviderBadgesResponse(
                snapshot.MeiVerified,
                AvailableTodayRule.IsOn(snapshot.AvailableToday, snapshot.AvailableTodaySetAt, clock.UtcNow, snapshot.TimeZone)),
            rated ? new ProviderRatingResponse(snapshot.RatingScore!.Value, snapshot.ReviewCount) : null,
            !rated,
            snapshot.ResponseTimeBucket,
            snapshot.CompletedOrders >= MinCompletedOrdersShown ? snapshot.CompletedOrders : null,
            snapshot.WhatsAppPhone,
            DistanceFrom(snapshot, query.Latitude, query.Longitude),
            snapshot.FirstPhotos,
            links.ProviderProfile(snapshot.Id));
    }

    private static double? DistanceFrom(ProviderPublicSnapshot snapshot, double? latitude, double? longitude) =>
        latitude is { } lat && longitude is { } lng
            ? GeoDistance.Rounded(GeoDistance.Kilometers(new Coordinates(lat, lng), new Coordinates(snapshot.PublicLatitude, snapshot.PublicLongitude)))
            : null;
}
