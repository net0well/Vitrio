using Vitrio.Domain.Common;

namespace Vitrio.Domain.Providers;

public sealed record ServiceDraft(string? Title, string? Description, Money? PriceFrom, PriceUnit Unit, bool OnQuote);
