using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Favorites.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandler(
    ICurrentUser currentUser,
    IFavoriteWritableRepository favorites,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveFavoriteCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        if (await favorites.GetForUserAsync(currentUser.Id, command.ProviderId, cancellationToken) is { } favorite)
        {
            favorites.Remove(favorite);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
