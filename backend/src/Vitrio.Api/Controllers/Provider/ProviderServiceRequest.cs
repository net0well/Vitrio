using Vitrio.Application.Providers.SetProviderServices;
using Vitrio.Domain.Providers;

namespace Vitrio.Api.Controllers.Provider;

public sealed record ProviderServiceRequest(string? Title, string? Description, decimal? PriceFrom, PriceUnit? PriceUnit, bool? OnQuote)
{
    public ServiceInput ToInput() => new(Title, Description, PriceFrom, PriceUnit, OnQuote ?? false);
}
