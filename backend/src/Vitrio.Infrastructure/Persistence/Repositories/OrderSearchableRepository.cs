using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Orders;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Storage;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class OrderSearchableRepository(AppDbContext db, PublicMedia media, IPrivateMediaStorage privateMedia) : IOrderSearchableRepository
{
    private static readonly OrderStatus[] OpenStatuses = [OrderStatus.Sent, OrderStatus.Quoted, OrderStatus.Scheduled];

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
