using Vitrio.Application.Orders.CreateOrderPhotoUpload;

namespace Vitrio.Api.Controllers.Orders;

public sealed record OrderPhotoUploadRequest(string? ContentType, long? ContentLength)
{
    public CreateOrderPhotoUploadCommand ToCommand() => new(ContentType ?? string.Empty, ContentLength ?? 0);
}
