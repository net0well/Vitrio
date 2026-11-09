using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class OrderWritableRepository(AppDbContext db) : IOrderWritableRepository
{
    private static readonly OrderStatus[] OpenStatuses = [OrderStatus.Sent, OrderStatus.Quoted, OrderStatus.Scheduled];

    private static readonly OrderStatus[] ServiceDoneStatuses =
        [OrderStatus.Completed, OrderStatus.PaymentConfirmed, OrderStatus.PaymentDisputed, OrderStatus.PaymentVoided];

    public Task<Order?> GetForCustomerAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken) =>
        db.Orders.Include(order => order.Photos)
            .SingleOrDefaultAsync(order => order.Id == orderId && order.CustomerId == customerId, cancellationToken);

    public Task<Order?> GetForProviderAsync(Guid orderId, Guid providerId, CancellationToken cancellationToken) =>
        db.Orders.Include(order => order.Photos)
            .SingleOrDefaultAsync(order => order.Id == orderId && order.ProviderId == providerId, cancellationToken);

    public Task<Order?> GetAsync(Guid orderId, CancellationToken cancellationToken) =>
        db.Orders.Include(order => order.Photos).SingleOrDefaultAsync(order => order.Id == orderId, cancellationToken);

    public Task<int> CountOpenForCustomerAsync(Guid customerId, CancellationToken cancellationToken) =>
        db.Orders.CountAsync(order => order.CustomerId == customerId && OpenStatuses.Contains(order.Status), cancellationToken);

    public Task<int> CountWaitingForCustomerAsync(Guid customerId, int categoryId, CancellationToken cancellationToken) =>
        db.Orders.CountAsync(
            order => order.CustomerId == customerId && order.CategoryId == categoryId && order.Status == OrderStatus.Sent,
            cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListDueToExpireAsync(
        DateTimeOffset now,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking()
            .Where(order => (order.Status == OrderStatus.Sent && order.RespondBy <= now)
                || (order.Status == OrderStatus.Quoted && order.QuoteValidUntil <= now))
            .Where(order => !excluded.Contains(order.Id))
            .OrderBy(order => order.Id)
            .Select(order => order.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListScheduledEndedBeforeAsync(
        DateTimeOffset endedBefore,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking()
            .Where(order => order.Status == OrderStatus.Scheduled && order.Schedule!.EndsAt <= endedBefore)
            .Where(order => !excluded.Contains(order.Id))
            .OrderBy(order => order.Id)
            .Select(order => order.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Guid>> ListWithPersonalDataEndedBeforeAsync(
        DateTimeOffset endedBefore,
        IReadOnlyCollection<Guid> excluded,
        int take,
        CancellationToken cancellationToken) =>
        await db.Orders.AsNoTracking()
            .Where(order => order.PersonalDataErasedAt == null)
            .Where(order => ServiceDoneStatuses.Contains(order.Status)
                ? order.Schedule!.EndsAt <= endedBefore
                : !OpenStatuses.Contains(order.Status) && order.ClosedAt <= endedBefore)
            .Where(order => !excluded.Contains(order.Id))
            .OrderBy(order => order.Id)
            .Select(order => order.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Order>> ListAwaitingAnswerForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Orders
            .Where(order => order.Status == OrderStatus.Sent || order.Status == OrderStatus.Quoted)
            .Where(order => order.CustomerId == userId
                || db.Providers.Any(provider => provider.Id == order.ProviderId && provider.UserId == userId))
            .ToListAsync(cancellationToken);

    public void Add(Order order) => db.Orders.Add(order);
}
