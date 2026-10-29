using Vitrio.Application.Providers.ConfirmPhoto;

namespace Vitrio.Api.Controllers.Provider;

public sealed record ConfirmPhotoRequest(string? ObjectKey, string? Caption)
{
    public ConfirmPhotoCommand ToCommand() => new(ObjectKey ?? string.Empty, Caption);
}
