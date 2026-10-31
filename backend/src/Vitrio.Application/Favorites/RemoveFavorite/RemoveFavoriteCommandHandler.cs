using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Favorites.RemoveFavorite;

public sealed class RemoveFavoriteCommandHandler(
    ICurrentUser currentUser,
    IFavoriteWritableRepository favorites,
    IUnitOfWork unitOfWork) : ICommandHandler<RemoveFavoriteCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveFavoriteCommand command, CancellationToken cancellationToken)
    {
        if (await favorites.GetForUserAsync(currentUser.Id, command.ProviderId, cancellationToken) is not { } favorite)
        {
            return Result.Success();
        }

        favorites.Remove(favorite);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Success();
        }

        return Result.Success();
    }
}
