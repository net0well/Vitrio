using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.DeletePhoto;

public sealed class DeletePhotoCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<DeletePhotoCommand, Result>
{
    public async ValueTask<Result> Handle(DeletePhotoCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var photo = provider.DeletePhoto(command.PhotoId, clock);
        if (photo.IsFailure)
        {
            return photo.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (photo.Value.UploadKey is { } uploadKey)
        {
            await storage.DeleteUploadAsync(uploadKey, cancellationToken);
        }

        foreach (var key in new[] { photo.Value.ObjectKey, photo.Value.ThumbnailKey }.OfType<string>())
        {
            await storage.DeletePublicAsync(key, cancellationToken);
        }

        return Result.Success();
    }
}
