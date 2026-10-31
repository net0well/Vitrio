using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Favorites.Repositories;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Favorites;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Favorites.AddFavorite;

public sealed class AddFavoriteCommandHandler(
    ICurrentUser currentUser,
    IProviderSearchableRepository providers,
    IFavoriteWritableRepository favorites,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<AddFavoriteCommand, Result>
{
    public async ValueTask<Result> Handle(AddFavoriteCommand command, CancellationToken cancellationToken)
    {
        var owner = await providers.FindPublishedOwnerAsync(command.ProviderId, cancellationToken);
        if (owner is null || owner == currentUser.Id)
        {
            return ProviderErrors.NotFound;
        }

        if (await favorites.GetForUserAsync(currentUser.Id, command.ProviderId, cancellationToken) is not null)
        {
            return Result.Success();
        }

        if (await favorites.CountForUserAsync(currentUser.Id, cancellationToken) >= Favorite.MaxPerUser)
        {
            return FavoriteErrors.LimitReached;
        }

        favorites.Add(Favorite.Create(currentUser.Id, command.ProviderId, clock));
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            return Result.Success();
        }

        return Result.Success();
    }
}
