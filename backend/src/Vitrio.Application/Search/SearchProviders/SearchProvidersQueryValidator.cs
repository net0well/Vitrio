using FluentValidation;
using Vitrio.Application.Search.Common;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Search.SearchProviders;

internal sealed class SearchProvidersQueryValidator : AbstractValidator<SearchProvidersQuery>
{
    public SearchProvidersQueryValidator()
    {
        Include(new SearchLocationValidator());
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
