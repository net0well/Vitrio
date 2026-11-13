namespace Vitrio.Domain.Providers;

public static class ProviderStats
{
    public const int ResponseSampleSize = 20;
    public const int MinimumResponseSample = 5;

    public static readonly TimeSpan Window = TimeSpan.FromDays(90);

    private static readonly (TimeSpan UpTo, string Bucket)[] ResponseBuckets =
    [
        (TimeSpan.FromMinutes(15), "15min"),
        (TimeSpan.FromMinutes(30), "30min"),
        (TimeSpan.FromHours(1), "1h"),
        (TimeSpan.FromHours(3), "3h"),
        (TimeSpan.FromDays(1), "1d")
    ];

    public static string? ResponseTimeBucket(IReadOnlyCollection<TimeSpan> responseDelays)
    {
        if (responseDelays.Count < MinimumResponseSample)
        {
            return null;
        }

        var median = Median(responseDelays);
        foreach (var (upTo, bucket) in ResponseBuckets)
        {
            if (median <= upTo)
            {
                return bucket;
            }
        }

        return null;
    }

    private static TimeSpan Median(IReadOnlyCollection<TimeSpan> values)
    {
        var ordered = values.Order().ToArray();
        var middle = ordered.Length / 2;
        return ordered.Length % 2 == 1 ? ordered[middle] : (ordered[middle - 1] + ordered[middle]) / 2;
    }
}
