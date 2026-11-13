namespace Vitrio.Application.Orders.Common;

public static class OrderPhotoKeys
{
    public static readonly TimeSpan ReadUrlValidity = TimeSpan.FromMinutes(10);

    public static string NewUpload(Guid customerId) => $"orders/{customerId:N}/{Guid.CreateVersion7():N}";

    public static bool BelongsTo(string? uploadKey, Guid customerId) =>
        uploadKey is not null
        && uploadKey.StartsWith($"orders/{customerId:N}/", StringComparison.Ordinal)
        && !uploadKey.Contains("..", StringComparison.Ordinal);

    public static string Stored(Guid orderId, Guid photoId) => $"orders/{orderId:N}/{photoId:N}.webp";
}
