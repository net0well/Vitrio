using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Favorites.AddFavorite;

public sealed record AddFavoriteCommand(Guid ProviderId) : ICommand<Result>;
