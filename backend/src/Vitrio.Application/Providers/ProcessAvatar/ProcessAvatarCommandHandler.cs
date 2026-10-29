using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.ProcessAvatar;

public sealed class ProcessAvatarCommandHandler(
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IImageProcessor images,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ProcessAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(ProcessAvatarCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByIdAsync(command.ProviderId, cancellationToken);
        if (provider is null || provider.AvatarUploadKey != command.UploadKey)
        {
            return Result.Success();
        }

        var upload = await storage.FindUploadAsync(command.UploadKey, cancellationToken);
        Result<byte[]> processed = Result<byte[]>.Success([]);
        if (upload is { Size: <= PhotoKeys.MaxUploadBytes })
        {
            await using var image = await storage.OpenUploadAsync(command.UploadKey, cancellationToken);
            processed = images.ProcessAvatar(image, upload.ContentType);
        }

        string? previous = null;
        if (upload is { Size: <= PhotoKeys.MaxUploadBytes } && processed.IsSuccess)
        {
            var key = PhotoKeys.Avatar(provider.Id);
            await storage.SavePublicAsync(key, processed.Value, PhotoKeys.WebpContentType, cancellationToken);
            previous = provider.CompleteAvatar(command.UploadKey, key, clock).Value;
        }
        else
        {
            provider.RejectAvatar(command.UploadKey, clock);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await storage.DeleteUploadAsync(command.UploadKey, cancellationToken);
        if (previous is not null)
        {
            await storage.DeletePublicAsync(previous, cancellationToken);
        }

        return Result.Success();
    }
}
