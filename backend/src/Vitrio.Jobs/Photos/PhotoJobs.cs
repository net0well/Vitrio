using Mediator;
using Vitrio.Application.Providers.CleanUpUploads;
using Vitrio.Application.Providers.ProcessAvatar;
using Vitrio.Application.Providers.ProcessPhoto;

namespace Vitrio.Jobs.Photos;

public sealed class PhotoJobs(ISender sender)
{
    public async Task ProcessPhotoAsync(Guid providerId, Guid photoId, CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new ProcessPhotoCommand(providerId, photoId), cancellationToken));

    public async Task ProcessAvatarAsync(Guid providerId, string uploadKey, CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new ProcessAvatarCommand(providerId, uploadKey), cancellationToken));

    public async Task CleanUpUploadsAsync(CancellationToken cancellationToken) =>
        JobResults.EnsureSucceeded(await sender.Send(new CleanUpUploadsCommand(), cancellationToken));
}
