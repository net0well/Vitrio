using System.Globalization;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Search.Common;
using Vitrio.Application.Search.Repositories;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Search.Services;

internal sealed class TopRatedProvidersService(
    IProviderListingSearchableRepository listings,
    IReadCache cache,
    IClock clock) : ITopRatedProvidersService
{
    public const int Shown = 10;

    private const int CellCandidates = 50;
    private const double CellDegrees = 0.02;
    private const double CellMarginMeters = 2_000;

    private static readonly TimeSpan Lifetime = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan LocalLifetime = TimeSpan.FromSeconds(30);

    public async Task<IReadOnlyList<ProviderSearchItem>> ListAsync(SearchArea area, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        if (area is not { IsNearMe: true, Point: { } point })
        {
            var cityCandidates = await CandidatesAsync($"home:city:{area.City!.IbgeCode}", area, 0, Shown, now, cancellationToken);
            return cityCandidates.Select(candidate => Finish(candidate, now, null)).ToList();
        }

        var cell = new Coordinates(Snap(point.Latitude), Snap(point.Longitude));
        var key = string.Create(CultureInfo.InvariantCulture, $"home:cell:{cell.Latitude:F2}:{cell.Longitude:F2}");
        var candidates = await CandidatesAsync(key, area with { Point = cell }, CellMarginMeters, CellCandidates, now, cancellationToken);
        return candidates
            .Select(candidate => (Candidate: candidate, Km: GeoDistance.Kilometers(point, new Coordinates(candidate.Latitude, candidate.Longitude))))
            .Where(serving => serving.Km * SearchDistances.MetersPerKm <= Math.Min(serving.Candidate.RadiusMeters, SearchDistances.DefaultMaxMeters))
            .OrderByDescending(serving => serving.Candidate.Item.Rating!.Score)
            .ThenBy(serving => serving.Km)
            .Take(Shown)
            .Select(serving => Finish(serving.Candidate, now, GeoDistance.Rounded(serving.Km)))
            .ToList();
    }

    private static double Snap(double degrees) => Math.Round(degrees / CellDegrees) * CellDegrees;

    private static ProviderSearchItem Finish(TopRatedCandidate candidate, DateTimeOffset now, double? distanceKm) =>
        candidate.Item with
        {
            DistanceKm = distanceKm,
            Badges = candidate.Item.Badges with
            {
                AvailableToday = AvailableTodayRule.IsOn(candidate.AvailableToday, candidate.AvailableTodaySetAt, now, candidate.TimeZone)
            }
        };

    private async Task<IReadOnlyList<TopRatedCandidate>> CandidatesAsync(
        string key,
        SearchArea area,
        double marginMeters,
        int take,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(
            key,
            token => new ValueTask<IReadOnlyList<TopRatedCandidate>>(listings.ListTopRatedAsync(area, marginMeters, take, now, token)),
            Lifetime,
            LocalLifetime,
            [],
            cancellationToken);
}
