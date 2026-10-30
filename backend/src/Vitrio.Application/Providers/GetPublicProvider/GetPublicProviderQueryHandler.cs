using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Services;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.GetPublicProvider;

public sealed class GetPublicProviderQueryHandler(
    IPublicProfileService profiles,
    IPublicLinks links,
    IClock clock) : IQueryHandler<GetPublicProviderQuery, Result<ProviderPublicResponse>>
{
    private const int MinCompletedOrdersShown = 5;

    public async ValueTask<Result<ProviderPublicResponse>> Handle(GetPublicProviderQuery query, CancellationToken cancellationToken)
    {
        var snapshot = await profiles.GetSnapshotAsync(query.ProviderId, cancellationToken);
        if (snapshot is null)
        {
            return ProviderErrors.NotFound;
        }

        var rated = snapshot.IsRated(RatingRule.MinReviews);
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
