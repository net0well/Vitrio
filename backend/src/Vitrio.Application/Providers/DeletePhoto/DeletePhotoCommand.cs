using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.DeletePhoto;

public sealed record DeletePhotoCommand(Guid PhotoId) : ICommand<Result>;
