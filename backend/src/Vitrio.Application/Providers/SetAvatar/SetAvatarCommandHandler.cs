using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.SetAvatar;

public sealed class SetAvatarCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IPhotoProcessingQueue queue,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(SetAvatarCommand command, CancellationToken cancellationToken)
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

        provider.RequestAvatar(command.ObjectKey, clock);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        queue.EnqueueAvatar(provider.Id, command.ObjectKey);
        return Result.Success();
    }
}
