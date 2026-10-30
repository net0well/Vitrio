using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.RemoveAvatar;

public sealed class RemoveAvatarCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IObjectStorage storage,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RemoveAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveAvatarCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var previous = provider.RemoveAvatar(clock);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        if (previous is not null)
        {
            await storage.DeletePublicAsync(previous, cancellationToken);
        }

        return Result.Success();
    }
}
