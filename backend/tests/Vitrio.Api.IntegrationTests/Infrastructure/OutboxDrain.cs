using Microsoft.Extensions.DependencyInjection;
using Vitrio.Infrastructure.Events;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public static class OutboxDrain
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    // The background dispatcher delivers one event at a time and the whole suite fills its queue, so a test that waits
    // for a handler also delivers events itself (concurrent delivery is safe: each one is reserved with skip locked).
    public static Task UntilAsync(this ApiFactory factory, Func<Task<bool>> condition, TimeSpan? timeout = null)
    {
        var processor = factory.Services.GetRequiredService<OutboxProcessor>();
        return Eventually.SucceedsAsync(
            async () =>
            {
                if (await condition())
                {
                    return true;
                }

                await processor.ProcessBatchAsync(TestContext.Current.CancellationToken);
                return await condition();
            },
            timeout ?? DefaultTimeout);
    }
}
