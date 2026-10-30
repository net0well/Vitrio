using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.CreatePhotoUpload;

public sealed record CreatePhotoUploadCommand(PhotoPurpose Purpose, string ContentType, long ContentLength) : ICommand<Result<PhotoUploadResponse>>;
