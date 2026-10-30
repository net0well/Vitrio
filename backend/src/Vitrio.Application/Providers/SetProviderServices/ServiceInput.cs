using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.SetProviderServices;

public sealed record ServiceInput(string? Title, string? Description, decimal? PriceFrom, PriceUnit? PriceUnit, bool OnQuote);
