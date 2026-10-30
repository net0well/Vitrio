using FluentValidation;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Search.SearchProviders;

internal sealed class SearchProvidersQueryValidator : AbstractValidator<SearchProvidersQuery>
{
    public SearchProvidersQueryValidator()
    {
        RuleFor(query => query.Latitude)
            .Must((query, latitude) => latitude is null == query.Longitude is null)
            .WithMessage("Informe a latitude e a longitude juntas.");
        RuleFor(query => query.Latitude)
            .InclusiveBetween(BrazilBounds.MinLatitude, BrazilBounds.MaxLatitude).When(query => query.Latitude is not null)
            .WithMessage("O ponto precisa estar no Brasil.");
        RuleFor(query => query.Longitude)
            .InclusiveBetween(BrazilBounds.MinLongitude, BrazilBounds.MaxLongitude).When(query => query.Longitude is not null)
            .WithMessage("O ponto precisa estar no Brasil.");
        RuleFor(query => query.CityIbgeCode)
            .NotNull().When(query => query.Latitude is null)
            .WithMessage("Use a sua localização ou escolha uma cidade.");
        RuleFor(query => query.Q)
            .Must(term => FreeText.Normalize(term).Length <= SearchProvidersQuery.MaxTermLength)
            .WithMessage($"Busque com até {SearchProvidersQuery.MaxTermLength} caracteres.");
        RuleFor(query => query.CategoryId).GreaterThan(0).When(query => query.CategoryId is not null);
        RuleFor(query => query.MaxDistanceKm)
            .InclusiveBetween(Provider.MinRadiusKm, Provider.MaxRadiusKm).When(query => query.MaxDistanceKm is not null);
        RuleFor(query => query.MaxDistanceKm)
            .Null().When(query => query.Latitude is null)
            .WithMessage(SearchErrors.NeedsLocation.Message);
        RuleFor(query => query.MinRating).InclusiveBetween(3m, 5m).When(query => query.MinRating is not null);
        RuleFor(query => query.MaxPriceFrom)
            .InclusiveBetween(1m, 100_000m).PrecisionScale(8, 2, true).When(query => query.MaxPriceFrom is not null);
        RuleFor(query => query.Sort).IsInEnum();
        RuleFor(query => query.Sort)
            .NotEqual(SearchSort.Distance).When(query => query.Latitude is null)
            .WithMessage(SearchErrors.NeedsLocation.Message);
        RuleFor(query => query.Page).InclusiveBetween(1, SearchProvidersQuery.MaxPage);
        RuleFor(query => query.PageSize).InclusiveBetween(1, SearchProvidersQuery.MaxPageSize);
    }
}
