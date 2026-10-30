using Mediator;
using Vitrio.Application.Providers.CleanUpUploads;
using Vitrio.Application.Providers.ProcessAvatar;
using Vitrio.Application.Providers.ProcessPhoto;
using Vitrio.Shared.Results;

namespace Vitrio.Jobs.Photos;

public sealed class PhotoJobs(ISender sender)
{
    public async Task ProcessPhotoAsync(Guid providerId, Guid photoId, CancellationToken cancellationToken) =>
        EnsureSucceeded(await sender.Send(new ProcessPhotoCommand(providerId, photoId), cancellationToken));

    public async Task ProcessAvatarAsync(Guid providerId, string uploadKey, CancellationToken cancellationToken) =>
        EnsureSucceeded(await sender.Send(new ProcessAvatarCommand(providerId, uploadKey), cancellationToken));

    public async Task CleanUpUploadsAsync(CancellationToken cancellationToken) =>
        EnsureSucceeded(await sender.Send(new CleanUpUploadsCommand(), cancellationToken));

    // Hangfire only retries a job that throws, so a failed command must surface as an exception.
    private static void EnsureSucceeded(Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException($"The job failed with '{result.Error!.Code}'.");
        }
    }
}
