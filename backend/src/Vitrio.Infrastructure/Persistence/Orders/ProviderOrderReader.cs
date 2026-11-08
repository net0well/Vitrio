using Microsoft.EntityFrameworkCore;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Orders;

namespace Vitrio.Infrastructure.Persistence.Orders;

internal sealed class ProviderOrderReader(AppDbContext db, IPrivateMediaStorage privateMedia)
{
    private const int LatestNewOrdersShown = 3;

    private static readonly OrderStatus[] OpenStatuses = [OrderStatus.Sent, OrderStatus.Quoted, OrderStatus.Scheduled];
    private static readonly OrderStatus[] DoneStatuses = [OrderStatus.Completed, OrderStatus.PaymentConfirmed, OrderStatus.PaymentDisputed];

    public async Task<OrderProviderView?> GetViewAsync(Guid orderId, Guid providerId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var order = await db.Orders
            .AsNoTracking()
            .Include(candidate => candidate.Photos)
            .SingleOrDefaultAsync(candidate => candidate.Id == orderId && candidate.ProviderId == providerId, cancellationToken);
        if (order is null)
        {
            return null;
        }

        var customer = await db.Users
            .AsNoTracking()
            .Where(user => user.Id == order.CustomerId)
            .Select(user => new { user.CreatedAt, Orders = db.Orders.Count(candidate => candidate.CustomerId == user.Id) })
            .SingleAsync(cancellationToken);
        var origin = await ProviderPointAsync(providerId, cancellationToken);
        var context = new OrderProviderContext(customer.CreatedAt, customer.Orders, DistanceKm(origin, order.Place.Latitude, order.Place.Longitude));
        return OrderProviderView.From(order, context, OrderPhotoResponse.ListFrom(order.Photos, ReadUrl), now);
    }

    public async Task<OrderProviderPage> ListAsync(
        Guid providerId,
        ProviderOrderTab tab,
        DateTimeOffset now,
        int skip,
        int take,
        CancellationToken cancellationToken)
    {
        var orders = db.Orders.AsNoTracking().Where(order => order.ProviderId == providerId);
        var sorted = tab switch
        {
            ProviderOrderTab.New => orders
                .Where(order => order.Status == OrderStatus.Sent)
                .OrderByDescending(order => order.CreatedAt)
                .ThenByDescending(order => order.Id),
            ProviderOrderTab.Ongoing => orders
                .Where(order => order.Status == OrderStatus.Quoted || order.Status == OrderStatus.Scheduled)
                .OrderBy(order => order.Status == OrderStatus.Scheduled ? 0 : 1)
                .ThenBy(order => order.Schedule!.StartsAt)
                .ThenBy(order => order.QuoteValidUntil)
                .ThenBy(order => order.Id),
            _ => orders
                .Where(order => !OpenStatuses.Contains(order.Status))
                .OrderByDescending(order => order.CreatedAt)
                .ThenByDescending(order => order.Id)
        };

        var rows = await sorted
            .Skip(skip)
            .Take(take + 1)
            .Select(order => new SummaryRow(
                order.Id,
                order.Status,
                order.Title,
                order.ServiceTitle,
                order.Description,
                order.CustomerDisplayName,
                order.Contact!.FullName,
                order.Place.Neighborhood,
                order.Place.Latitude,
                order.Place.Longitude,
                order.PreferredDay,
                order.PreferredPeriod,
                order.CreatedAt,
                order.RespondBy,
                order.Schedule,
                order.QuoteValidUntil,
                order.CancelledBy,
                order.AddressReleasedAt,
                order.PersonalDataErasedAt,
                order.Photos.Count(photo => photo.Status == OrderPhotoStatus.Ready || photo.Status == OrderPhotoStatus.Processing)))
            .ToListAsync(cancellationToken);

        var origin = await ProviderPointAsync(providerId, cancellationToken);
        var items = rows.Take(take).Select(row => ToSummary(row, origin, now)).ToList();
        return new OrderProviderPage(items, rows.Count > take);
    }

    public async Task<IReadOnlyList<AgendaItemResponse>> ListAgendaAsync(
        Guid providerId,
        DateOnly from,
        DateOnly to,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var rows = await db.Orders
            .AsNoTracking()
            .Where(order => order.ProviderId == providerId
                && (((order.Status == OrderStatus.Scheduled || DoneStatuses.Contains(order.Status))
                        && order.Schedule!.Date >= from && order.Schedule.Date <= to)
                    || (order.Status == OrderStatus.Quoted && order.QuoteValidUntil > now
                        && order.Quote!.Schedule.Date >= from && order.Quote.Schedule.Date <= to)))
            .Select(order => new AgendaRow(
                order.Id,
                order.Status,
                order.Title,
                order.CustomerDisplayName,
                order.Contact!.FullName,
                order.Place.Neighborhood,
                order.Schedule,
                order.Quote!.Schedule,
                order.QuoteValidUntil,
                order.AddressReleasedAt,
                order.PersonalDataErasedAt))
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => ToAgendaItem(row, now))
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Start, StringComparer.Ordinal)
            .ThenBy(item => item.OrderId)
            .ToList();
    }

    public async Task<ProviderOrderNumbers> GetNumbersAsync(
        Guid providerId,
        DateTimeOffset monthStart,
        DateTimeOffset nextMonthStart,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var newOrders = await db.Orders.CountAsync(
            order => order.ProviderId == providerId && order.Status == OrderStatus.Sent,
            cancellationToken);
        var servicesThisMonth = await db.Orders.CountAsync(
            order => order.ProviderId == providerId
                && DoneStatuses.Contains(order.Status)
                && order.CompletedAt >= monthStart
                && order.CompletedAt < nextMonthStart,
            cancellationToken);
        var latest = await ListAsync(providerId, ProviderOrderTab.New, now, 0, LatestNewOrdersShown, cancellationToken);
        return new ProviderOrderNumbers(newOrders, servicesThisMonth, latest.Items);
    }

    private static OrderProviderSummary ToSummary(SummaryRow row, Coordinates? origin, DateTimeOffset now) => new(
        row.Id,
        row.Status,
        row.Title,
        row.ServiceTitle,
        row.Description,
        CustomerName(row.DisplayName, row.FullName, row.Status, row.AddressReleasedAt, row.PersonalDataErasedAt, row.Schedule, now),
        row.Neighborhood,
        DistanceKm(origin, row.Latitude, row.Longitude),
        row.PreferredDay,
        row.PreferredPeriod,
        row.CreatedAt,
        row.RespondBy,
        row.Schedule is null ? null : ServiceTimeResponse.From(row.Schedule),
        row.QuoteValidUntil,
        row.CancelledBy,
        row.PhotoCount);

    private static AgendaItemResponse ToAgendaItem(AgendaRow row, DateTimeOffset now)
    {
        var kind = row.Status switch
        {
            OrderStatus.Scheduled => AgendaItemKind.Scheduled,
            OrderStatus.Quoted => AgendaItemKind.Quoted,
            _ => AgendaItemKind.Done
        };
        var time = ServiceTimeResponse.From(kind == AgendaItemKind.Quoted ? row.QuoteSchedule! : row.Schedule!);
        return new AgendaItemResponse(
            row.Id,
            kind,
            time.Date,
            time.Start,
            time.End,
            row.Title,
            CustomerName(row.DisplayName, row.FullName, row.Status, row.AddressReleasedAt, row.PersonalDataErasedAt, row.Schedule, now),
            row.Neighborhood,
            kind == AgendaItemKind.Quoted ? row.QuoteValidUntil : null);
    }

    private static string CustomerName(
        string displayName,
        string? fullName,
        OrderStatus status,
        DateTimeOffset? addressReleasedAt,
        DateTimeOffset? personalDataErasedAt,
        ServiceSchedule? schedule,
        DateTimeOffset now) =>
        fullName is not null && Order.IsContactVisible(addressReleasedAt, status, personalDataErasedAt, schedule?.EndsAt, now)
            ? fullName
            : displayName;

    private static double? DistanceKm(Coordinates? origin, double latitude, double longitude) =>
        origin is null ? null : GeoDistance.Rounded(GeoDistance.Kilometers(origin, new Coordinates(latitude, longitude)));

    private async Task<Coordinates?> ProviderPointAsync(Guid providerId, CancellationToken cancellationToken)
    {
        var point = await db.Providers
            .AsNoTracking()
            .Where(provider => provider.Id == providerId)
            .Select(provider => new { provider.PublicLatitude, provider.PublicLongitude })
            .SingleOrDefaultAsync(cancellationToken);
        return point is { PublicLatitude: { } latitude, PublicLongitude: { } longitude } ? new Coordinates(latitude, longitude) : null;
    }

    private string ReadUrl(string objectKey) => privateMedia.CreateReadUrl(objectKey, OrderPhotoKeys.ReadUrlValidity);

    private sealed record SummaryRow(
        Guid Id,
        OrderStatus Status,
        string Title,
        string ServiceTitle,
        string Description,
        string DisplayName,
        string? FullName,
        string Neighborhood,
        double Latitude,
        double Longitude,
        DateOnly PreferredDay,
        PreferredPeriod PreferredPeriod,
        DateTimeOffset CreatedAt,
        DateTimeOffset RespondBy,
        ServiceSchedule? Schedule,
        DateTimeOffset? QuoteValidUntil,
        OrderParty? CancelledBy,
        DateTimeOffset? AddressReleasedAt,
        DateTimeOffset? PersonalDataErasedAt,
        int PhotoCount);

    private sealed record AgendaRow(
        Guid Id,
        OrderStatus Status,
        string Title,
        string DisplayName,
        string? FullName,
        string Neighborhood,
        ServiceSchedule? Schedule,
        ServiceSchedule? QuoteSchedule,
        DateTimeOffset? QuoteValidUntil,
        DateTimeOffset? AddressReleasedAt,
        DateTimeOffset? PersonalDataErasedAt);
}
