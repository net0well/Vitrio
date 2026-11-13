using Mediator;
using Vitrio.Application.Providers.RecheckMeiVerifications;

namespace Vitrio.Jobs.Providers;

public sealed class MeiJobs(ISender sender)
{
    private const int BatchSize = 200;

    private static readonly TimeSpan PauseBetweenLookups = TimeSpan.FromSeconds(1);

    public async Task RecheckAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new RecheckMeiVerificationsCommand(BatchSize, PauseBetweenLookups), cancellationToken));
}
