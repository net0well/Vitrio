using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.ProcessPhoto;

public sealed class ProcessPhotoCommandHandler(
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IImageProcessor images,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ProcessPhotoCommand, Result>
{
    public async ValueTask<Result> Handle(ProcessPhotoCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByIdAsync(command.ProviderId, cancellationToken);
        var photo = provider?.Photos.FirstOrDefault(candidate => candidate.Id == command.PhotoId);
        if (provider is null || photo is not { Status: PhotoStatus.Processing, UploadKey: { } uploadKey })
        {
            return Result.Success();
        }

        var upload = await storage.FindUploadAsync(uploadKey, cancellationToken);
        Result<ProcessedPhoto> processed = upload switch
        {
            null => Error.Validation(PhotoRejections.NotAnImage, PhotoRejections.MessageFor(PhotoRejections.NotAnImage)),
            { Size: > PhotoKeys.MaxUploadBytes } => Error.Validation(PhotoRejections.TooLarge, PhotoRejections.MessageFor(PhotoRejections.TooLarge)),
            _ => await ProcessAsync(uploadKey, upload.ContentType, cancellationToken)
        };

        if (processed.IsFailure)
        {
            provider.RejectPhoto(photo.Id, processed.Error!.Code, clock);
        }
        else
        {
            var full = PhotoKeys.Full(provider.Id, photo.Id);
            var thumbnail = PhotoKeys.Thumbnail(provider.Id, photo.Id);
            await storage.SavePublicAsync(full, processed.Value.Full, PhotoKeys.WebpContentType, cancellationToken);
            await storage.SavePublicAsync(thumbnail, processed.Value.Thumbnail, PhotoKeys.WebpContentType, cancellationToken);
            provider.CompletePhoto(photo.Id, full, thumbnail, clock);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await storage.DeleteUploadAsync(uploadKey, cancellationToken);
        return Result.Success();
    }

    private async Task<Result<ProcessedPhoto>> ProcessAsync(string uploadKey, string? contentType, CancellationToken cancellationToken)
    {
        await using var image = await storage.OpenUploadAsync(uploadKey, cancellationToken);
        return images.ProcessPhoto(image, contentType);
    }
}
