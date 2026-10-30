using Vitrio.Application.Providers.CreatePhotoUpload;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.Controllers.Provider;

public sealed record PhotoUploadRequest(PhotoPurpose? Purpose, string? ContentType, long? ContentLength)
{
    public CreatePhotoUploadCommand ToCommand() => new(Purpose ?? PhotoPurpose.Portfolio, ContentType ?? string.Empty, ContentLength ?? 0);
}
