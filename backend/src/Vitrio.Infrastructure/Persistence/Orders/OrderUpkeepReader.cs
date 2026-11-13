using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;

namespace Vitrio.Infrastructure.Persistence.Orders;

internal sealed class OrderUpkeepReader(AppDbContext db)
{
    private static readonly OrderStatus[] ServicesDoneStatuses = [OrderStatus.Completed, OrderStatus.PaymentConfirmed, OrderStatus.PaymentDisputed];

    public async Task<IReadOnlyList<TimeSpan>> ListResponseDelaysAsync(
        Guid providerId,
        DateTimeOffset since,
        int take,
        CancellationToken cancellationToken)
    {
        var rows = await db.Orders
            .AsNoTracking()
            .Where(order => order.ProviderId == providerId && order.CreatedAt >= since)
            .Where(order => order.FirstResponseAt != null || order.Status == OrderStatus.Expired)
            .OrderByDescending(order => order.CreatedAt)
            .Take(take)
            .Select(order => new { order.CreatedAt, order.FirstResponseAt, order.RespondBy })
            .ToListAsync(cancellationToken);
        return [.. rows.Select(row => Order.ResponseDelay(row.CreatedAt, row.FirstResponseAt, row.RespondBy))];
    }

    public Task<int> CountServicesDoneAsync(Guid providerId, CancellationToken cancellationToken) =>
        db.Orders.AsNoTracking().CountAsync(
            order => order.ProviderId == providerId && ServicesDoneStatuses.Contains(order.Status),
            cancellationToken);

    public async Task<int> CountLateCancellationsAsync(Guid providerId, DateTimeOffset since, CancellationToken cancellationToken)
    {
        var cancellations = await db.Orders
            .AsNoTracking()
            .Where(order => order.ProviderId == providerId && order.CancelledBy == OrderParty.Provider && order.CancelledAt >= since)
            .Where(order => order.Schedule != null)
            .Select(order => new { order.Schedule!.StartsAt, CancelledAt = order.CancelledAt!.Value })
            .ToListAsync(cancellationToken);
        return cancellations.Count(cancellation => Order.IsLateCancellation(cancellation.StartsAt, cancellation.CancelledAt));
    }

    public async Task<IReadOnlyList<Guid>> ListProvidersWithOrdersBetweenAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken) =>
        await db.Orders
            .AsNoTracking()
            .Where(order => (order.CreatedAt >= from && order.CreatedAt < to)
                || (order.CancelledBy == OrderParty.Provider && order.CancelledAt >= from && order.CancelledAt < to))
            .Select(order => order.ProviderId)
            .Distinct()
            .ToListAsync(cancellationToken);

    public Task<bool> HasScheduledForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Orders.AsNoTracking().AnyAsync(
            order => order.Status == OrderStatus.Scheduled
                && (order.CustomerId == userId
                    || db.Providers.Any(provider => provider.Id == order.ProviderId && provider.UserId == userId)),
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListProcessingPhotoIdsAsync(Guid orderId, CancellationToken cancellationToken) =>
        await db.OrderPhotos
            .AsNoTracking()
            .Where(photo => photo.OrderId == orderId && photo.Status == OrderPhotoStatus.Processing)
            .OrderBy(photo => photo.CreatedAt)
            .ThenBy(photo => photo.Id)
            .Select(photo => photo.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PendingOrderPhoto>> ListPhotosProcessingSinceAsync(
        DateTimeOffset createdBefore,
        CancellationToken cancellationToken) =>
        await db.OrderPhotos
            .AsNoTracking()
            .Where(photo => photo.Status == OrderPhotoStatus.Processing && photo.CreatedAt < createdBefore)
            .Select(photo => new PendingOrderPhoto(photo.OrderId, photo.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<string>> ListPendingUploadKeysAsync(CancellationToken cancellationToken) =>
        await db.OrderPhotos
            .AsNoTracking()
            .Where(photo => photo.Status == OrderPhotoStatus.Processing)
            .Select(photo => photo.UploadKey)
            .ToListAsync(cancellationToken);
}
