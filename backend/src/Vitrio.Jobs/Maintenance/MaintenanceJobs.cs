using Mediator;
using Vitrio.Application.Common.DeleteExpiredIdempotencyKeys;

namespace Vitrio.Jobs.Maintenance;

public sealed class MaintenanceJobs(ISender sender)
{
    public async Task DeleteExpiredIdempotencyKeysAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new DeleteExpiredIdempotencyKeysCommand(), cancellationToken));
}
