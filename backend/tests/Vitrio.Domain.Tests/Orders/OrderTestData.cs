using Vitrio.Domain.Orders;
using Vitrio.Domain.Tests.Support;

namespace Vitrio.Domain.Tests.Orders;

internal static class OrderTestData
{
    public const string SaoPaulo = "America/Sao_Paulo";

    public static readonly Guid CustomerId = Guid.Parse("0193c9a8-0000-7000-8000-00000000c001");
    public static readonly Guid ProviderId = Guid.Parse("0193c9a8-0000-7000-8000-00000000b001");

    public static readonly OrderAddress Address = new("Rua Rui Barbosa", "123", "apto 12", "14870-000");
    public static readonly OrderContact Contact = new("Mariana Souza", "+5516991234567");
    public static readonly OrderPlace Place = new(3524303, "Jaboticabal", "SP", "Centro", SaoPaulo, -21.2547, -48.3221, false);

    public static DateTimeOffset Local(int year, int month, int day, int hour, int minute = 0) =>
        new(year, month, day, hour, minute, 0, TimeSpan.FromHours(-3));

    public static TestClock ClockAt(DateTimeOffset localTime) => new(localTime.ToUniversalTime());

    public static DateOnly Today(TestClock clock) =>
        DateOnly.FromDateTime(clock.UtcNow.ToOffset(TimeSpan.FromHours(-3)).DateTime);

    public static OrderRequest Request(
        DateOnly day,
        PreferredPeriod period = PreferredPeriod.Afternoon,
        string title = "Chuveiro desarmando o disjuntor",
        string description = "O chuveiro desarma o disjuntor quando ligo no modo inverno.",
        int photos = 0) =>
        new(
            ServiceId: null,
            ServiceTitle: "Chuveiro",
            CategoryId: 1,
            Title: title,
            Description: description,
            PreferredDay: day,
            PreferredPeriod: period,
            Address: Address,
            Contact: Contact,
            Place: Place,
            PhotoUploadKeys: [.. Enumerable.Range(1, photos).Select(index => $"uploads/foto-{index}")]);

    public static QuoteTerms Terms(
        DateOnly date,
        string start = "14:00",
        int durationMinutes = 60,
        decimal amount = 180m,
        bool materialIncluded = true,
        string? materialDetails = "disjuntor 40 A") =>
        new(QuoteType.FixedPrice, amount, date, TimeOnly.Parse(start, System.Globalization.CultureInfo.InvariantCulture), durationMinutes, materialIncluded, materialDetails, "Troco o disjuntor e reviso a fiação.");

    public static Order Sent(TestClock clock, DateOnly? day = null, PreferredPeriod period = PreferredPeriod.Afternoon, int photos = 0) =>
        Order.Create(CustomerId, ProviderId, Request(day ?? Today(clock).AddDays(3), period, photos: photos), clock).Value;

    public static Order Quoted(TestClock clock, DateOnly? date = null, string start = "14:00", int durationMinutes = 60)
    {
        var order = Sent(clock);
        order.SendQuote(Terms(date ?? Today(clock).AddDays(3), start, durationMinutes), clock).ThrowIfFailed();
        return order;
    }

    public static Order Scheduled(TestClock clock, DateOnly? date = null, string start = "14:00", int durationMinutes = 60)
    {
        var order = Quoted(clock, date, start, durationMinutes);
        order.AcceptQuote(order.Quote!.Version, clock).ThrowIfFailed();
        order.ClearDomainEvents();
        return order;
    }

    private static void ThrowIfFailed(this Shared.Results.Result result)
    {
        if (result.IsFailure)
        {
            throw new InvalidOperationException(result.Error!.Code);
        }
    }
}
