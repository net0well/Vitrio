using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.CreatePhotoUpload;

public sealed class CreatePhotoUploadCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IClock clock) : ICommandHandler<CreatePhotoUploadCommand, Result<PhotoUploadResponse>>
{
    private static readonly TimeSpan Validity = TimeSpan.FromMinutes(5);

    public async ValueTask<Result<PhotoUploadResponse>> Handle(CreatePhotoUploadCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        if (command.Purpose == PhotoPurpose.Portfolio && !provider.CanReceivePhoto)
        {
            return ProviderErrors.TooManyPhotos;
        }

        var key = PhotoKeys.NewUpload(provider.Id);
        var url = storage.CreateUploadUrl(key, command.ContentType, command.ContentLength, Validity);
        return new PhotoUploadResponse(url, key, clock.UtcNow.Add(Validity));
    }
}
