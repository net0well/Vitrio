using System.Threading.RateLimiting;

namespace Vitrio.Api.RateLimiting;

public sealed class ChainedRateLimiter(params RateLimiter[] limiters) : RateLimiter
{
    public override TimeSpan? IdleDuration
    {
        get
        {
            TimeSpan? shortest = null;
            foreach (var limiter in limiters)
            {
                if (limiter.IdleDuration is not { } idle)
                {
                    return null;
                }

                shortest = shortest is null || idle < shortest ? idle : shortest;
            }

            return shortest;
        }
    }

    public override RateLimiterStatistics? GetStatistics()
    {
        var statistics = limiters.Select(limiter => limiter.GetStatistics()).OfType<RateLimiterStatistics>().ToList();
        if (statistics.Count == 0)
        {
            return null;
        }

        return new RateLimiterStatistics
        {
            CurrentAvailablePermits = statistics.Min(item => item.CurrentAvailablePermits),
            CurrentQueuedCount = statistics.Sum(item => item.CurrentQueuedCount),
            TotalFailedLeases = statistics.Sum(item => item.TotalFailedLeases),
            TotalSuccessfulLeases = statistics.Min(item => item.TotalSuccessfulLeases)
        };
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        var acquired = new List<RateLimitLease>(limiters.Length);
        foreach (var limiter in limiters)
        {
            var lease = limiter.AttemptAcquire(permitCount);
            if (!lease.IsAcquired)
            {
                Release(acquired);
                return lease;
            }

            acquired.Add(lease);
        }

        return new ChainedLease(acquired);
    }

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        var acquired = new List<RateLimitLease>(limiters.Length);
        foreach (var limiter in limiters)
        {
            var lease = await limiter.AcquireAsync(permitCount, cancellationToken);
            if (!lease.IsAcquired)
            {
                Release(acquired);
                return lease;
            }

            acquired.Add(lease);
        }

        return new ChainedLease(acquired);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var limiter in limiters)
            {
                limiter.Dispose();
            }
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        foreach (var limiter in limiters)
        {
            await limiter.DisposeAsync();
        }
    }

    private static void Release(List<RateLimitLease> leases)
    {
        foreach (var lease in leases)
        {
            lease.Dispose();
        }
    }

    private sealed class ChainedLease(List<RateLimitLease> leases) : RateLimitLease
    {
        public override bool IsAcquired => true;

        public override IEnumerable<string> MetadataNames => [];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = null;
            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Release(leases);
            }

            base.Dispose(disposing);
        }
    }
}
