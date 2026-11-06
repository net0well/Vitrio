using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderPhotoResponse(Guid Id, OrderPhotoStatus Status, string? Url)
{
    public static IReadOnlyList<OrderPhotoResponse> ListFrom(IEnumerable<OrderPhoto> photos, Func<string, string> readUrl) =>
        photos
            .Where(photo => photo.Status is OrderPhotoStatus.Ready or OrderPhotoStatus.Processing)
            .OrderBy(photo => photo.CreatedAt)
            .ThenBy(photo => photo.Id)
            .Select(photo => new OrderPhotoResponse(
                photo.Id,
                photo.Status,
                photo is { Status: OrderPhotoStatus.Ready, ObjectKey: { } key } ? readUrl(key) : null))
            .ToList();
}
