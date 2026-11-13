using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Common.CleanUpUploads;

public sealed class CleanUpUploadsCommandHandler(
    IProviderSearchableRepository providers,
    IOrderSearchableRepository orders,
    IObjectStorage storage,
    IPhotoProcessingQueue queue,
    IClock clock) : ICommandHandler<CleanUpUploadsCommand, Result>
{
    private static readonly TimeSpan StuckAfter = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AbandonedAfter = TimeSpan.FromHours(24);

    public async ValueTask<Result> Handle(CleanUpUploadsCommand command, CancellationToken cancellationToken)
    {
        var stuckSince = clock.UtcNow - StuckAfter;
        foreach (var photo in await providers.ListPhotosProcessingSinceAsync(stuckSince, cancellationToken))
        {
            queue.EnqueuePhoto(photo.ProviderId, photo.PhotoId);
        }

        foreach (var photo in await orders.ListPhotosProcessingSinceAsync(stuckSince, cancellationToken))
        {
            queue.EnqueueOrderPhoto(photo.OrderId, photo.PhotoId);
        }

        var pending = new HashSet<string>(await providers.ListPendingUploadKeysAsync(cancellationToken), StringComparer.Ordinal);
        pending.UnionWith(await orders.ListPendingUploadKeysAsync(cancellationToken));
        foreach (var key in await storage.ListUploadsCreatedBeforeAsync(clock.UtcNow - AbandonedAfter, cancellationToken))
        {
            if (!pending.Contains(key))
            {
                await storage.DeleteUploadAsync(key, cancellationToken);
            }
        }

        return Result.Success();
    }
}
