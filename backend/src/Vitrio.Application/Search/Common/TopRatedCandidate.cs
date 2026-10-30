namespace Vitrio.Application.Search.Common;

public sealed record TopRatedCandidate(
    ProviderSearchItem Item,
    double Latitude,
    double Longitude,
    int RadiusMeters,
    bool AvailableToday,
    DateTimeOffset? AvailableTodaySetAt,
    string TimeZone);
