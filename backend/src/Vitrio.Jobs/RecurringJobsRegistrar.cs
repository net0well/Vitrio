using Hangfire;
using Microsoft.Extensions.Hosting;
using Vitrio.Jobs.Photos;
using Vitrio.Jobs.Providers;

namespace Vitrio.Jobs;

internal sealed class RecurringJobsRegistrar(IRecurringJobManager recurringJobs) : IHostedService
{
    public const string PhotoUploadsCleanup = "photo-uploads-cleanup";
    public const string MeiVerifications = "mei-verifications";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobs.AddOrUpdate<PhotoJobs>(PhotoUploadsCleanup, photos => photos.CleanUpUploadsAsync(CancellationToken.None), Cron.Hourly());
        recurringJobs.AddOrUpdate<MeiJobs>(MeiVerifications, mei => mei.RecheckAsync(CancellationToken.None), Cron.Hourly());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
