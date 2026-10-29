using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.ReorderPhotos;

public sealed class ReorderPhotosCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<ReorderPhotosCommand, Result>
{
    public async ValueTask<Result> Handle(ReorderPhotosCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var result = provider.ReorderPhotos(command.PhotoIds, clock);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
