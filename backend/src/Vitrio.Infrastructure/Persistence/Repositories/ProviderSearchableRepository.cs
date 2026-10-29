using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Storage;
using Vitrio.Shared.Time;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class ProviderSearchableRepository(AppDbContext db, PublicMedia media) : IProviderSearchableRepository
{
    private const int MetersPerKm = 1000;
    private const int FirstPhotosShown = 6;

    private static readonly ProviderStatus[] ActiveStatuses = [ProviderStatus.Draft, ProviderStatus.Published, ProviderStatus.Paused];

    public Task<bool> HasProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Providers.AsNoTracking().AnyAsync(
            provider => provider.UserId == userId && provider.Status != ProviderStatus.Removed,
            cancellationToken);

    public Task<bool> HasActiveProfileAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Providers.AsNoTracking().AnyAsync(
            provider => provider.UserId == userId && ActiveStatuses.Contains(provider.Status),
            cancellationToken);

    public async Task<ProviderPrivateResponse?> GetPrivateAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var row = await db.Providers
            .AsNoTracking()
            .Where(provider => provider.UserId == userId && provider.Status != ProviderStatus.Removed)
            .Select(provider => new
            {
                provider.Id,
                provider.Status,
                provider.DisplayName,
                provider.Headline,
                provider.About,
                provider.YearsOfExperience,
                provider.AvatarKey,
                provider.ContactPhone,
                provider.WhatsAppEnabled,
                provider.HandlesEmergencies,
                provider.AvailableToday,
                provider.AvailableTodaySetAt,
                provider.BaseCep,
                provider.BaseStreet,
                provider.BaseNumber,
                provider.BaseComplement,
                provider.BaseNeighborhood,
                provider.ServiceRadiusMeters,
                provider.LocationApproximate,
                City = db.Cities
                    .Where(city => city.IbgeCode == provider.CityIbgeCode)
                    .Select(city => new { city.IbgeCode, city.Name, city.State, city.TimeZone })
                    .FirstOrDefault(),
                Categories = provider.Categories
                    .Join(db.Categories, chosen => chosen.CategoryId, category => category.Id, (chosen, category) =>
                        new ProviderCategoryResponse(category.Id, category.Name, chosen.IsPrimary))
                    .ToList(),
                Services = provider.Services
                    .OrderBy(service => service.SortOrder)
                    .Select(service => new { service.Id, service.Title, service.Description, service.PriceFromCents, service.PriceUnit, service.OnQuote })
                    .ToList(),
                Neighborhoods = provider.Neighborhoods.OrderBy(neighborhood => neighborhood.Name).Select(neighborhood => neighborhood.Name).ToList(),
                Availability = provider.Availability
                    .OrderBy(slot => slot.DayOfWeek)
                    .ThenBy(slot => slot.StartMinute)
                    .Select(slot => new { slot.DayOfWeek, slot.StartMinute, slot.EndMinute })
                    .ToList()
            })
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var serviceArea = row.City is null
            ? null
            : new ServiceAreaResponse(
                row.BaseCep!,
                row.BaseStreet,
                row.BaseNumber!,
                row.BaseComplement,
                row.BaseNeighborhood,
                new CitySummary(row.City.IbgeCode, row.City.Name, row.City.State),
                row.ServiceRadiusMeters / MetersPerKm,
                row.Neighborhoods,
                row.LocationApproximate);

        return new ProviderPrivateResponse(
            row.Id,
            row.Status,
            row.DisplayName,
            row.Headline,
            row.About,
            row.YearsOfExperience,
            media.UrlOrNull(row.AvatarKey),
            row.ContactPhone,
            row.WhatsAppEnabled,
            row.HandlesEmergencies,
            AvailableTodayRule.IsOn(row.AvailableToday, row.AvailableTodaySetAt, now, row.City?.TimeZone),
            row.Categories.OrderByDescending(category => category.IsPrimary).ThenBy(category => category.Name).ToList(),
            serviceArea,
            row.Services
                .Select(service => new ProviderServiceResponse(
                    service.Id,
                    service.Title,
                    service.Description,
                    service.PriceFromCents / 100m,
                    service.PriceUnit,
                    service.OnQuote))
                .ToList(),
            row.Availability
                .Select(slot => new AvailabilitySlotResponse(slot.DayOfWeek, ClockTime.Format(slot.StartMinute), ClockTime.Format(slot.EndMinute)))
                .ToList(),
            PublishChecklist.Missing(row.About is not null, row.Categories.Count, serviceArea is not null, row.Services.Count, row.ContactPhone is not null));
    }

    public async Task<ProviderPublicSnapshot?> GetPublicSnapshotAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var row = await db.Providers
            .AsNoTracking()
            .Where(provider => provider.Id == providerId
                && provider.Status == ProviderStatus.Published
                && db.Users.Any(user => user.Id == provider.UserId && user.Status == UserStatus.Active))
            .Select(provider => new
            {
                provider.Id,
                provider.DisplayName,
                provider.Headline,
                provider.About,
                provider.YearsOfExperience,
                provider.AvatarKey,
                City = db.Cities
                    .Where(city => city.IbgeCode == provider.CityIbgeCode)
                    .Select(city => new { city.IbgeCode, city.Name, city.State, city.TimeZone })
                    .First(),
                Photos = provider.Photos
                    .Where(photo => photo.Status == PhotoStatus.Active)
                    .OrderBy(photo => photo.SortOrder)
                    .Take(FirstPhotosShown)
                    .Select(photo => new { photo.Id, photo.ObjectKey, photo.ThumbnailKey, photo.Caption })
                    .ToList(),
                Neighborhoods = provider.Neighborhoods.OrderBy(neighborhood => neighborhood.Name).Select(neighborhood => neighborhood.Name).ToList(),
                provider.ServiceRadiusMeters,
                Categories = provider.Categories
                    .Join(db.Categories, chosen => chosen.CategoryId, category => category.Id, (chosen, category) =>
                        new ProviderCategoryResponse(category.Id, category.Name, chosen.IsPrimary))
                    .ToList(),
                Services = provider.Services
                    .OrderBy(service => service.SortOrder)
                    .Select(service => new { service.Id, service.Title, service.Description, service.PriceFromCents, service.PriceUnit, service.OnQuote })
                    .ToList(),
                Availability = provider.Availability
                    .OrderBy(slot => slot.DayOfWeek)
                    .ThenBy(slot => slot.StartMinute)
                    .Select(slot => new { slot.DayOfWeek, slot.StartMinute, slot.EndMinute })
                    .ToList(),
                provider.HandlesEmergencies,
                provider.RatingScore,
                provider.ReviewCount,
                provider.ResponseTimeBucket,
                provider.CompletedOrders,
                provider.WhatsAppEnabled,
                provider.ContactPhone,
                provider.PublicLatitude,
                provider.PublicLongitude,
                provider.AvailableToday,
                provider.AvailableTodaySetAt
            })
            .AsSplitQuery()
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        return new ProviderPublicSnapshot(
            row.Id,
            row.DisplayName,
            row.Headline,
            row.About,
            row.YearsOfExperience,
            media.UrlOrNull(row.AvatarKey),
            new CitySummary(row.City.IbgeCode, row.City.Name, row.City.State),
            row.City.TimeZone,
            row.Neighborhoods,
            row.ServiceRadiusMeters / MetersPerKm,
            row.Categories.OrderByDescending(category => category.IsPrimary).ThenBy(category => category.Name).ToList(),
            row.Services
                .Select(service => new ProviderServiceResponse(
                    service.Id,
                    service.Title,
                    service.Description,
                    service.PriceFromCents / 100m,
                    service.PriceUnit,
                    service.OnQuote))
                .ToList(),
            row.Availability
                .Select(slot => new AvailabilitySlotResponse(slot.DayOfWeek, ClockTime.Format(slot.StartMinute), ClockTime.Format(slot.EndMinute)))
                .ToList(),
            row.HandlesEmergencies,
            false,
            row.RatingScore,
            row.ReviewCount,
            row.ResponseTimeBucket,
            row.CompletedOrders,
            row.WhatsAppEnabled ? row.ContactPhone : null,
            row.PublicLatitude!.Value,
            row.PublicLongitude!.Value,
            row.AvailableToday,
            row.AvailableTodaySetAt,
            row.Photos.Select(photo => new PublicPhotoResponse(photo.Id, media.Url(photo.ObjectKey!), media.Url(photo.ThumbnailKey!), photo.Caption)).ToList());
    }

    public async Task<IReadOnlyList<ProviderPhotoResponse>?> ListOwnPhotosAsync(Guid userId, CancellationToken cancellationToken)
    {
        var provider = await db.Providers
            .AsNoTracking()
            .Where(candidate => candidate.UserId == userId && candidate.Status != ProviderStatus.Removed)
            .Select(candidate => new
            {
                Photos = candidate.Photos
                    .OrderBy(photo => photo.SortOrder)
                    .Select(photo => new { photo.Id, photo.ObjectKey, photo.ThumbnailKey, photo.Caption, photo.Status, photo.RejectionReason })
                    .ToList()
            })
            .SingleOrDefaultAsync(cancellationToken);

        return provider?.Photos
            .Select(photo => new ProviderPhotoResponse(
                photo.Id,
                media.UrlOrNull(photo.ObjectKey),
                media.UrlOrNull(photo.ThumbnailKey),
                photo.Caption,
                photo.Status,
                photo.Status == PhotoStatus.Rejected ? PhotoRejections.MessageFor(photo.RejectionReason) : null))
            .ToList();
    }

    public async Task<IReadOnlyList<PublicPhotoResponse>?> GetPublicPhotosAsync(Guid providerId, int skip, int take, CancellationToken cancellationToken)
    {
        var published = await db.Providers.AsNoTracking().AnyAsync(
            provider => provider.Id == providerId
                && provider.Status == ProviderStatus.Published
                && db.Users.Any(user => user.Id == provider.UserId && user.Status == UserStatus.Active),
            cancellationToken);
        if (!published)
        {
            return null;
        }

        var photos = await db.PortfolioPhotos
            .AsNoTracking()
            .Where(photo => photo.ProviderId == providerId && photo.Status == PhotoStatus.Active)
            .OrderBy(photo => photo.SortOrder)
            .Skip(skip)
            .Take(take)
            .Select(photo => new { photo.Id, photo.ObjectKey, photo.ThumbnailKey, photo.Caption })
            .ToListAsync(cancellationToken);
        return photos.Select(photo => new PublicPhotoResponse(photo.Id, media.Url(photo.ObjectKey!), media.Url(photo.ThumbnailKey!), photo.Caption)).ToList();
    }

    public async Task<IReadOnlyList<PendingPhoto>> ListPhotosProcessingSinceAsync(DateTimeOffset createdBefore, CancellationToken cancellationToken) =>
        await db.PortfolioPhotos
            .AsNoTracking()
            .Where(photo => photo.Status == PhotoStatus.Processing && photo.CreatedAt < createdBefore)
            .Select(photo => new PendingPhoto(photo.ProviderId, photo.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<string>> ListPendingUploadKeysAsync(CancellationToken cancellationToken)
    {
        var photos = await db.PortfolioPhotos
            .AsNoTracking()
            .Where(photo => photo.Status == PhotoStatus.Processing && photo.UploadKey != null)
            .Select(photo => photo.UploadKey!)
            .ToListAsync(cancellationToken);
        var avatars = await db.Providers
            .AsNoTracking()
            .Where(provider => provider.AvatarUploadKey != null)
            .Select(provider => provider.AvatarUploadKey!)
            .ToListAsync(cancellationToken);
        return photos.Concat(avatars).ToHashSet(StringComparer.Ordinal);
    }

    public Task<Guid?> FindIdByUserAsync(Guid userId, CancellationToken cancellationToken) =>
        db.Providers
            .AsNoTracking()
            .Where(provider => provider.UserId == userId)
            .OrderByDescending(provider => provider.CreatedAt)
            .Select(provider => (Guid?)provider.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<ProviderDashboardResponse?> GetDashboardAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var row = await db.Providers
            .AsNoTracking()
            .Where(provider => provider.UserId == userId && provider.Status != ProviderStatus.Removed)
            .Select(provider => new
            {
                provider.Status,
                HasAbout = provider.About != null,
                CategoryCount = provider.Categories.Count,
                HasServiceArea = provider.CityIbgeCode != null,
                ServiceCount = provider.Services.Count,
                HasContactPhone = provider.ContactPhone != null,
                HasAvailability = provider.Availability.Any(),
                provider.AvailableToday,
                provider.AvailableTodaySetAt,
                TimeZone = db.Cities.Where(city => city.IbgeCode == provider.CityIbgeCode).Select(city => city.TimeZone).FirstOrDefault()
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return null;
        }

        var missing = PublishChecklist.Missing(row.HasAbout, row.CategoryCount, row.HasServiceArea, row.ServiceCount, row.HasContactPhone);
        return new ProviderDashboardResponse(
            row.Status,
            PublishChecklist.Completeness(missing, row.HasAvailability),
            PublishChecklist.NextStep(missing, row.Status, row.HasAvailability),
            null,
            AvailableTodayRule.IsOn(row.AvailableToday, row.AvailableTodaySetAt, now, row.TimeZone));
    }
}
