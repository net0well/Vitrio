using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Locations.Common;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Time;

namespace Vitrio.Infrastructure.Persistence.Repositories;

internal sealed class ProviderSearchableRepository(AppDbContext db) : IProviderSearchableRepository
{
    private const int MetersPerKm = 1000;

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
                City = db.Cities
                    .Where(city => city.IbgeCode == provider.CityIbgeCode)
                    .Select(city => new { city.IbgeCode, city.Name, city.State, city.TimeZone })
                    .First(),
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
            null,
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
            []);
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
