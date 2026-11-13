using Vitrio.Domain.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Domain.Orders;

public sealed record Quote
{
    public const int MaxMaterialDetailsLength = 80;
    public const int MaxMessageLength = 500;

    private static readonly Money MinimumAmount = new(100);
    private static readonly Money MaximumAmount = new(10_000_000);

    private Quote()
    {
    }

    public QuoteType Type { get; private init; }

    public Money Amount { get; private init; }

    public ServiceSchedule Schedule { get; private init; } = null!;

    public bool MaterialIncluded { get; private init; }

    public string? MaterialDetails { get; private init; }

    public string? Message { get; private init; }

    public DateTimeOffset SentAt { get; private init; }

    public int Version { get; private init; }

    internal static Result<Quote> Create(QuoteTerms terms, string timeZoneId, int version, DateTimeOffset sentAt)
    {
        if (Money.FromReais(terms.Amount) is not { } amount || amount.Cents < MinimumAmount.Cents || amount.Cents > MaximumAmount.Cents)
        {
            return OrderErrors.InvalidAmount;
        }

        var schedule = ServiceSchedule.Create(terms.Date, terms.Start, terms.DurationMinutes, timeZoneId);
        if (schedule.IsFailure)
        {
            return schedule.Error!;
        }

        var materialDetails = FreeText.Normalize(terms.MaterialDetails);
        if (materialDetails.Length > MaxMaterialDetailsLength || (!terms.MaterialIncluded && materialDetails.Length > 0))
        {
            return OrderErrors.InvalidMaterialDetails;
        }

        var message = FreeText.Normalize(terms.Message);
        if (message.Length > MaxMessageLength)
        {
            return OrderErrors.InvalidMessage;
        }

        return new Quote
        {
            Type = terms.Type,
            Amount = amount,
            Schedule = schedule.Value,
            MaterialIncluded = terms.MaterialIncluded,
            MaterialDetails = materialDetails.Length > 0 ? materialDetails : null,
            Message = message.Length > 0 ? message : null,
            SentAt = sentAt,
            Version = version
        };
    }
}
