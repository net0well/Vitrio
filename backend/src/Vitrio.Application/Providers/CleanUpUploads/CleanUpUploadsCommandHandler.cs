using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.CleanUpUploads;

public sealed class CleanUpUploadsCommandHandler(
    IProviderSearchableRepository providers,
    IObjectStorage storage,
    IPhotoProcessingQueue queue,
    IClock clock) : ICommandHandler<CleanUpUploadsCommand, Result>
{
    private static readonly TimeSpan StuckAfter = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan AbandonedAfter = TimeSpan.FromHours(24);

    public async ValueTask<Result> Handle(CleanUpUploadsCommand command, CancellationToken cancellationToken)
    {
        foreach (var photo in await providers.ListPhotosProcessingSinceAsync(clock.UtcNow - StuckAfter, cancellationToken))
        {
            queue.EnqueuePhoto(photo.ProviderId, photo.PhotoId);
        }

        var pending = await providers.ListPendingUploadKeysAsync(cancellationToken);
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
