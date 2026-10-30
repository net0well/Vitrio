using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.RenamePhoto;

public sealed record RenamePhotoCommand(Guid PhotoId, string? Caption) : ICommand<Result>;
