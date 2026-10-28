using System.Threading.RateLimiting;
using Vitrio.Api.RateLimiting;

namespace Vitrio.Api.IntegrationTests.RateLimiting;

public sealed class ChainedRateLimiterTests
{
    [Fact]
    public void AttemptAcquire_WhenEveryWindowHasPermits_Succeeds()
    {
        using var limiter = new ChainedRateLimiter(Window(2, TimeSpan.FromMinutes(1)), Window(5, TimeSpan.FromHours(1)));

        using var lease = limiter.AttemptAcquire();

        Assert.True(lease.IsAcquired);
    }

    [Fact]
    public void AttemptAcquire_WhenShortWindowIsExhausted_IsRejectedWithRetryAfter()
    {
        using var limiter = new ChainedRateLimiter(Window(1, TimeSpan.FromMinutes(1)), Window(5, TimeSpan.FromHours(1)));
        using var first = limiter.AttemptAcquire();

        using var second = limiter.AttemptAcquire();

        Assert.False(second.IsAcquired);
        Assert.True(second.TryGetMetadata(MetadataName.RetryAfter, out _));
    }

    [Fact]
    public async Task AcquireAsync_WhenLongWindowIsExhausted_IsRejected()
    {
        using var limiter = new ChainedRateLimiter(Window(5, TimeSpan.FromMinutes(1)), Window(2, TimeSpan.FromHours(1)));
        using var first = await limiter.AcquireAsync(cancellationToken: TestContext.Current.CancellationToken);
        using var second = await limiter.AcquireAsync(cancellationToken: TestContext.Current.CancellationToken);

        using var third = await limiter.AcquireAsync(cancellationToken: TestContext.Current.CancellationToken);

        Assert.False(third.IsAcquired);
    }

    [Fact]
    public void IdleDuration_WhileAnyWindowIsInUse_IsNull()
    {
        using var limiter = new ChainedRateLimiter(Window(5, TimeSpan.FromMinutes(1)), Window(5, TimeSpan.FromHours(1)));

        using var lease = limiter.AttemptAcquire();

        Assert.Null(limiter.IdleDuration);
    }

    private static FixedWindowRateLimiter Window(int permitLimit, TimeSpan window) => new(new FixedWindowRateLimiterOptions
    {
        PermitLimit = permitLimit,
        Window = window,
        QueueLimit = 0,
        AutoReplenishment = false
    });
}
