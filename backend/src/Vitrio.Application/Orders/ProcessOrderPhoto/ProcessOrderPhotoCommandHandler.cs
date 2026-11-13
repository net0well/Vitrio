using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ProcessOrderPhoto;

public sealed class ProcessOrderPhotoCommandHandler(
    IOrderWritableRepository orders,
    IObjectStorage storage,
    IPrivateMediaStorage privateMedia,
    IImageProcessor images,
    IUnitOfWork unitOfWork) : ICommandHandler<ProcessOrderPhotoCommand, Result>
{
    public async ValueTask<Result> Handle(ProcessOrderPhotoCommand command, CancellationToken cancellationToken)
    {
        var order = await orders.GetAsync(command.OrderId, cancellationToken);
        var photo = order?.Photos.FirstOrDefault(candidate => candidate.Id == command.PhotoId);
        if (order is null || photo is not { Status: OrderPhotoStatus.Processing })
        {
            return Result.Success();
        }

        var processed = await ProcessAsync(photo.UploadKey, cancellationToken);
        if (processed is null)
        {
            order.FailPhoto(photo.Id);
        }
        else
        {
            var objectKey = OrderPhotoKeys.Stored(order.Id, photo.Id);
            await privateMedia.SaveAsync(objectKey, processed, PhotoKeys.WebpContentType, cancellationToken);
            order.CompletePhoto(photo.Id, objectKey);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await storage.DeleteUploadAsync(photo.UploadKey, cancellationToken);
        return Result.Success();
    }

    private async Task<byte[]?> ProcessAsync(string uploadKey, CancellationToken cancellationToken)
    {
        var upload = await storage.FindUploadAsync(uploadKey, cancellationToken);
        if (upload is not { Size: <= PhotoKeys.MaxUploadBytes })
        {
            return null;
        }

        await using var image = await storage.OpenUploadAsync(uploadKey, cancellationToken);
        var processed = images.ProcessOrderPhoto(image, upload.ContentType);
        return processed.IsSuccess ? processed.Value : null;
    }
}
