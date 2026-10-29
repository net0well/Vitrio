using Hangfire;
using Microsoft.Extensions.Hosting;
using Vitrio.Jobs.Photos;

namespace Vitrio.Jobs;

internal sealed class RecurringJobsRegistrar(IRecurringJobManager recurringJobs) : IHostedService
{
    public const string PhotoUploadsCleanup = "photo-uploads-cleanup";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobs.AddOrUpdate<PhotoJobs>(PhotoUploadsCleanup, photos => photos.CleanUpUploadsAsync(CancellationToken.None), Cron.Hourly());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
