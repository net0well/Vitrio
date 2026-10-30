namespace Vitrio.Application.Abstractions;

public interface IPhotoProcessingQueue
{
    void EnqueuePhoto(Guid providerId, Guid photoId);

    void EnqueueAvatar(Guid providerId, string uploadKey);
}
