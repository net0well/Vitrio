using Hangfire;
using Vitrio.Application.Abstractions;

namespace Vitrio.Jobs.Photos;

internal sealed class HangfirePhotoProcessingQueue(IBackgroundJobClient jobs) : IPhotoProcessingQueue
{
    public void EnqueuePhoto(Guid providerId, Guid photoId) =>
        jobs.Enqueue<PhotoJobs>(photo => photo.ProcessPhotoAsync(providerId, photoId, CancellationToken.None));

    public void EnqueueAvatar(Guid providerId, string uploadKey) =>
        jobs.Enqueue<PhotoJobs>(photo => photo.ProcessAvatarAsync(providerId, uploadKey, CancellationToken.None));
}
