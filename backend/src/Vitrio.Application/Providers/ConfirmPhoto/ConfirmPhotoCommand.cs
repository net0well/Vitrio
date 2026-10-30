using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ConfirmPhoto;

public sealed record ConfirmPhotoCommand(string ObjectKey, string? Caption) : ICommand<Result<PhotoAcceptedResponse>>;
