using Vitrio.Application.Providers.RenamePhoto;

namespace Vitrio.Api.Controllers.Provider;

public sealed record PhotoCaptionRequest(string? Caption)
{
    public RenamePhotoCommand ToCommand(Guid photoId) => new(photoId, Caption);
}
