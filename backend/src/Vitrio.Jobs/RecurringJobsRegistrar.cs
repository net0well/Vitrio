using Hangfire;
using Microsoft.Extensions.Hosting;
using Vitrio.Jobs.Maintenance;
using Vitrio.Jobs.Orders;
using Vitrio.Jobs.Photos;
using Vitrio.Jobs.Providers;

namespace Vitrio.Jobs;

internal sealed class RecurringJobsRegistrar(IRecurringJobManager recurringJobs) : IHostedService
{
    public const string PhotoUploadsCleanup = "photo-uploads-cleanup";
    public const string MeiVerifications = "mei-verifications";
    public const string IdempotencyKeysCleanup = "idempotency-keys-cleanup";
    public const string OrderExpirations = "order-expirations";
    public const string ForgottenOrders = "forgotten-orders";
    public const string OrderPersonalDataErasure = "order-personal-data-erasure";
    public const string ProviderStats = "provider-stats";

    private const string EveryFiveMinutes = "*/5 * * * *";

    public Task StartAsync(CancellationToken cancellationToken)
    {
        recurringJobs.AddOrUpdate<PhotoJobs>(PhotoUploadsCleanup, photos => photos.CleanUpUploadsAsync(CancellationToken.None), Cron.Hourly());
        recurringJobs.AddOrUpdate<MeiJobs>(MeiVerifications, mei => mei.RecheckAsync(CancellationToken.None), Cron.Hourly());
        recurringJobs.AddOrUpdate<MaintenanceJobs>(IdempotencyKeysCleanup, maintenance => maintenance.DeleteExpiredIdempotencyKeysAsync(CancellationToken.None), Cron.Daily());
        recurringJobs.AddOrUpdate<OrderJobs>(OrderExpirations, orders => orders.ExpireAsync(CancellationToken.None), EveryFiveMinutes);
        recurringJobs.AddOrUpdate<OrderJobs>(ForgottenOrders, orders => orders.CompleteForgottenAsync(CancellationToken.None), Cron.Hourly());
        recurringJobs.AddOrUpdate<OrderJobs>(OrderPersonalDataErasure, orders => orders.ErasePersonalDataAsync(CancellationToken.None), Cron.Daily());
        recurringJobs.AddOrUpdate<OrderJobs>(ProviderStats, orders => orders.RecalculateProviderStatsAsync(CancellationToken.None), Cron.Daily());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
