using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CreateOrderPhotoUpload;

public sealed record CreateOrderPhotoUploadCommand(string ContentType, long ContentLength) : ICommand<Result<PhotoUploadResponse>>;
