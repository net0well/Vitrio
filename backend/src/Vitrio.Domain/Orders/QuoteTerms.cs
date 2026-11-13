namespace Vitrio.Domain.Orders;

public sealed record QuoteTerms(QuoteType Type, decimal Amount, DateOnly Date, TimeOnly Start, int DurationMinutes, bool MaterialIncluded, string? MaterialDetails, string? Message);
