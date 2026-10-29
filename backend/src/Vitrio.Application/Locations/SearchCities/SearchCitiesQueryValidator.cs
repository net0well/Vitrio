using FluentValidation;
using Vitrio.Domain.Common;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Locations.SearchCities;

internal sealed class SearchCitiesQueryValidator : AbstractValidator<SearchCitiesQuery>
{
    public const int MaxSearchLength = 60;

    public SearchCitiesQueryValidator()
    {
        RuleFor(query => query.Search)
            .Must(search => SearchText.Normalize(search).Length is >= 2 and <= MaxSearchLength)
            .WithMessage("Digite pelo menos 2 letras do nome da cidade.");
        RuleFor(query => query.State)
            .Must(BrazilStates.IsState).When(query => query.State is not null)
            .WithMessage("Informe a UF com 2 letras.");
        RuleFor(query => query.Limit)
            .InclusiveBetween(1, SearchCitiesQuery.MaxLimit).When(query => query.Limit is not null);
    }
}
