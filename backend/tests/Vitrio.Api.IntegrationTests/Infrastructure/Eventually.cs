using System.Diagnostics;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public static class Eventually
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMilliseconds(100);

    public static async Task<TimeSpan> SucceedsAsync(Func<Task<bool>> condition, TimeSpan? timeout = null)
    {
        var watch = Stopwatch.StartNew();
        var limit = timeout ?? TimeSpan.FromSeconds(10);
        while (watch.Elapsed < limit)
        {
            if (await condition())
            {
                return watch.Elapsed;
            }

            await Task.Delay(PollInterval, TestContext.Current.CancellationToken);
        }

        Assert.Fail($"The condition was not met within {limit.TotalSeconds} s.");
        return watch.Elapsed;
    }
}
