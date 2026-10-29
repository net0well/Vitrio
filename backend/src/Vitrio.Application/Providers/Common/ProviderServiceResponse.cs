using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderServiceResponse(Guid Id, string Title, string? Description, decimal? PriceFrom, PriceUnit PriceUnit, bool OnQuote);
