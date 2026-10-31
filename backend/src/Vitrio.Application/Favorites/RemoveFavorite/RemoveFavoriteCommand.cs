using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Favorites.RemoveFavorite;

public sealed record RemoveFavoriteCommand(Guid ProviderId) : ICommand<Result>;
