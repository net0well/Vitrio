using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CopyOrderPhotos;

public sealed class CopyOrderPhotosCommandHandler(
    ICurrentUser currentUser,
    IOrderSearchableRepository orders,
    IPrivateMediaStorage media) : ICommandHandler<CopyOrderPhotosCommand, Result<IReadOnlyList<OrderPhotoCopyResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<OrderPhotoCopyResponse>>> Handle(CopyOrderPhotosCommand command, CancellationToken cancellationToken)
    {
        var stored = await orders.FindStoredPhotosAsync(command.OrderId, currentUser.Id, cancellationToken);
        if (stored is null)
        {
            return OrderErrors.NotFound;
        }

        if (stored.Removed)
        {
            return OrderErrors.PhotosRemoved;
        }

        var copies = new List<OrderPhotoCopyResponse>(stored.ObjectKeys.Count);
        foreach (var objectKey in stored.ObjectKeys)
        {
            var uploadKey = OrderPhotoKeys.NewUpload(currentUser.Id);
            await media.CopyToUploadsAsync(objectKey, uploadKey, cancellationToken);
            copies.Add(new OrderPhotoCopyResponse(uploadKey, media.CreateReadUrl(objectKey, OrderPhotoKeys.ReadUrlValidity)));
        }

        return Result<IReadOnlyList<OrderPhotoCopyResponse>>.Success(copies);
    }
}
