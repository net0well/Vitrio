using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.CreateOrderPhotoUpload;

public sealed class CreateOrderPhotoUploadCommandHandler(ICurrentUser currentUser, IObjectStorage storage, IClock clock)
    : ICommandHandler<CreateOrderPhotoUploadCommand, Result<PhotoUploadResponse>>
{
    private static readonly TimeSpan Validity = TimeSpan.FromMinutes(5);

    public ValueTask<Result<PhotoUploadResponse>> Handle(CreateOrderPhotoUploadCommand command, CancellationToken cancellationToken)
    {
        var key = OrderPhotoKeys.NewUpload(currentUser.Id);
        var url = storage.CreateUploadUrl(key, command.ContentType, command.ContentLength, Validity);
        return ValueTask.FromResult<Result<PhotoUploadResponse>>(new PhotoUploadResponse(url, key, clock.UtcNow.Add(Validity)));
    }
}
