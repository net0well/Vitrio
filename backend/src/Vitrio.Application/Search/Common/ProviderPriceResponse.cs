using Vitrio.Domain.Providers;

namespace Vitrio.Application.Search.Common;

public sealed record ProviderPriceResponse(decimal From, PriceUnit Unit);
