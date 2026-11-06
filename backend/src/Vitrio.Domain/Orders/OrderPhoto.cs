namespace Vitrio.Domain.Orders;

public sealed class OrderPhoto
{
    private OrderPhoto()
    {
    }

    public Guid Id { get; private set; }

    public Guid OrderId { get; private set; }

    public string UploadKey { get; private set; } = string.Empty;

    public string? ObjectKey { get; private set; }

    public OrderPhotoStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    internal static OrderPhoto Create(Guid orderId, string uploadKey, DateTimeOffset createdAt) => new()
    {
        Id = Guid.CreateVersion7(),
        OrderId = orderId,
        UploadKey = uploadKey,
        Status = OrderPhotoStatus.Processing,
        CreatedAt = createdAt
    };

    internal void MarkReady(string objectKey)
    {
        ObjectKey = objectKey;
        Status = OrderPhotoStatus.Ready;
    }

    internal void MarkFailed() => Status = OrderPhotoStatus.Failed;

    internal void MarkRemoved()
    {
        ObjectKey = null;
        Status = OrderPhotoStatus.Removed;
    }
}
