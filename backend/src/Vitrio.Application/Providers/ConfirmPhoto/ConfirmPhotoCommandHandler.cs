using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.ConfirmPhoto;

public sealed class ConfirmPhotoCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IPhotoProcessingQueue queue,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ConfirmPhotoCommand, Result<PhotoAcceptedResponse>>
{
    public async ValueTask<Result<PhotoAcceptedResponse>> Handle(ConfirmPhotoCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        if (!PhotoKeys.BelongsTo(command.ObjectKey, provider.Id) || await storage.FindUploadAsync(command.ObjectKey, cancellationToken) is null)
        {
            return ProviderErrors.UploadNotFound;
        }

        var photo = provider.AddPhoto(command.ObjectKey, command.Caption, clock);
        if (photo.IsFailure)
        {
            return photo.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        queue.EnqueuePhoto(provider.Id, photo.Value.Id);
        return new PhotoAcceptedResponse(photo.Value.Id, photo.Value.Status);
    }
}
