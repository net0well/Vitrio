using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Orders;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence.Orders;
using Vitrio.Infrastructure.Storage;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class OrderSearchableRepository(
    AppDbContext db,
    PublicMedia media,
    IPrivateMediaStorage privateMedia,
    ProviderOrderReader providerOrders,
    OrderUpkeepReader upkeep) : IOrderSearchableRepository
{
    private static readonly OrderStatus[] OpenStatuses = [OrderStatus.Sent, OrderStatus.Quoted, OrderStatus.Scheduled];

    public Task<OrderProviderView?> GetProviderViewAsync(Guid orderId, Guid providerId, DateTimeOffset now, CancellationToken cancellationToken) =>
        providerOrders.GetViewAsync(orderId, providerId, now, cancellationToken);

    public Task<OrderProviderPage> ListForProviderAsync(
        Guid providerId,
        ProviderOrderTab tab,
        DateTimeOffset now,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        providerOrders.ListAsync(providerId, tab, now, skip, take, cancellationToken);

    public Task<IReadOnlyList<AgendaItemResponse>> ListAgendaAsync(
        Guid providerId,
        DateOnly from,
        DateOnly to,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        providerOrders.ListAgendaAsync(providerId, from, to, now, cancellationToken);

    public Task<ProviderOrderNumbers> GetProviderNumbersAsync(
        Guid providerId,
        DateTimeOffset monthStart,
        DateTimeOffset nextMonthStart,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        providerOrders.GetNumbersAsync(providerId, monthStart, nextMonthStart, now, cancellationToken);

    public Task<IReadOnlyList<TimeSpan>> ListResponseDelaysAsync(Guid providerId, DateTimeOffset since, int take, CancellationToken cancellationToken) =>
        upkeep.ListResponseDelaysAsync(providerId, since, take, cancellationToken);

    public Task<int> CountServicesDoneAsync(Guid providerId, CancellationToken cancellationToken) =>
        upkeep.CountServicesDoneAsync(providerId, cancellationToken);

    public Task<int> CountLateCancellationsAsync(Guid providerId, DateTimeOffset since, CancellationToken cancellationToken) =>
        upkeep.CountLateCancellationsAsync(providerId, since, cancellationToken);

    public Task<IReadOnlyList<Guid>> ListProvidersWithOrdersBetweenAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken) =>
        upkeep.ListProvidersWithOrdersBetweenAsync(from, to, cancellationToken);

    public Task<bool> HasScheduledForUserAsync(Guid userId, CancellationToken cancellationToken) =>
        upkeep.HasScheduledForUserAsync(userId, cancellationToken);

    public Task<IReadOnlyList<Guid>> ListProcessingPhotoIdsAsync(Guid orderId, CancellationToken cancellationToken) =>
        upkeep.ListProcessingPhotoIdsAsync(orderId, cancellationToken);

    public Task<IReadOnlyList<PendingOrderPhoto>> ListPhotosProcessingSinceAsync(DateTimeOffset createdBefore, CancellationToken cancellationToken) =>
        upkeep.ListPhotosProcessingSinceAsync(createdBefore, cancellationToken);

    public Task<IReadOnlyCollection<string>> ListPendingUploadKeysAsync(CancellationToken cancellationToken) =>
        upkeep.ListPendingUploadKeysAsync(cancellationToken);

    public async Task<OrderCustomerView?> GetCustomerViewAsync(Guid orderId, Guid customerId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(candidate => candidate.Photos)
            .SingleOrDefaultAsync(candidate => candidate.Id == orderId && candidate.CustomerId == customerId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var provider = await ProviderCardAsync(order.ProviderId, cancellationToken);
        return OrderCustomerView.From(order, provider, OrderPhotoResponse.ListFrom(order.Photos, ReadUrl), now);
    }

    public async Task<OrderCustomerPage> ListForCustomerAsync(Guid customerId, bool open, int skip, int take, CancellationToken cancellationToken)
    {
        var rows = await db.Orders
            .AsNoTracking()
            .Where(order => order.CustomerId == customerId && OpenStatuses.Contains(order.Status) == open)
            .OrderByDescending(order => order.CreatedAt)
            .ThenByDescending(order => order.Id)
            .Skip(skip)
            .Take(take + 1)
            .Select(order => new
            {
                order.Id,
                order.Title,
                order.CategoryId,
                ProviderName = db.Providers.Where(provider => provider.Id == order.ProviderId).Select(provider => provider.DisplayName).First(),
                order.Status,
                order.CreatedAt,
                order.Schedule
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Take(take)
            .Select(row => new OrderCustomerSummary(
                row.Id,
                row.Title,
                row.CategoryId,
                row.ProviderName,
                row.Status,
                row.CreatedAt,
                row.Schedule is null ? null : ServiceTimeResponse.From(row.Schedule)))
            .ToList();
        return new OrderCustomerPage(items, rows.Count > take);
    }

    public async Task<OrderStoredPhotos?> FindStoredPhotosAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken)
    {
        var row = await db.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId && order.CustomerId == customerId)
            .Select(order => new
            {
                Removed = order.PersonalDataErasedAt != null,
                Keys = order.Photos
                    .Where(photo => photo.Status == OrderPhotoStatus.Ready && photo.ObjectKey != null)
                    .OrderBy(photo => photo.CreatedAt)
                    .ThenBy(photo => photo.Id)
                    .Select(photo => photo.ObjectKey!)
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);
        return row is null ? null : new OrderStoredPhotos(row.Removed, row.Keys);
    }

    public Task<OrderSuggestionContext?> FindSuggestionContextAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken) =>
        db.Orders
            .AsNoTracking()
            .Where(order => order.Id == orderId && order.CustomerId == customerId)
            .Select(order => new OrderSuggestionContext(order.Status, order.ProviderId, order.CategoryId, order.Place.Latitude, order.Place.Longitude))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<OrderProviderCard> ProviderCardAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var row = await db.Providers
            .AsNoTracking()
            .Where(provider => provider.Id == providerId)
            .Select(provider => new
            {
                provider.Id,
                provider.DisplayName,
                provider.Headline,
                provider.AvatarKey,
                provider.RatingScore,
                provider.ReviewCount,
                provider.WhatsAppEnabled,
                provider.ContactPhone
            })
            .SingleAsync(cancellationToken);

        var rated = RatingRule.IsRated(row.RatingScore, row.ReviewCount);
        return new OrderProviderCard(
            row.Id,
            row.DisplayName,
            row.Headline,
            DisplayName.FromStored(row.DisplayName).Initials,
            media.UrlOrNull(row.AvatarKey),
            rated ? new ProviderRatingResponse(row.RatingScore!.Value, row.ReviewCount) : null,
            !rated,
            row.WhatsAppEnabled ? row.ContactPhone : null);
    }

    private string ReadUrl(string objectKey) => privateMedia.CreateReadUrl(objectKey, OrderPhotoKeys.ReadUrlValidity);
}
