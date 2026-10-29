using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Vitrio.Infrastructure.Events;

internal sealed class OutboxDispatcher(
    OutboxProcessor processor,
    IOptions<OutboxOptions> options,
    ILogger<OutboxDispatcher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            return;
        }

        using var timer = new PeriodicTimer(settings.PollInterval);
        var nextStatisticsAt = DateTimeOffset.MinValue;
        do
        {
            try
            {
                while (await processor.ProcessBatchAsync(stoppingToken) >= settings.BatchSize)
                {
                }

                if (TimeProvider.System.GetUtcNow() >= nextStatisticsAt)
                {
                    await processor.RefreshBacklogAsync(stoppingToken);
                    nextStatisticsAt = TimeProvider.System.GetUtcNow() + settings.StatisticsInterval;
                }
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogWarning("Outbox dispatch failed ({ErrorType}); trying again on the next tick", exception.GetType().Name);
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
