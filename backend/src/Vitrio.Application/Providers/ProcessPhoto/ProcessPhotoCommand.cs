using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ProcessPhoto;

public sealed record ProcessPhotoCommand(Guid ProviderId, Guid PhotoId) : ICommand<Result>;
