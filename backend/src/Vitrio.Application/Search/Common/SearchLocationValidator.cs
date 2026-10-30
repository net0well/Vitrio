using FluentValidation;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Search.Common;

internal sealed class SearchLocationValidator : AbstractValidator<ISearchLocation>
{
    public SearchLocationValidator()
    {
        RuleFor(location => location.Latitude)
            .Must((location, latitude) => latitude is null == location.Longitude is null)
            .WithMessage("Informe a latitude e a longitude juntas.");
        RuleFor(location => location.Latitude)
            .InclusiveBetween(BrazilBounds.MinLatitude, BrazilBounds.MaxLatitude).When(location => location.Latitude is not null)
            .WithMessage("O ponto precisa estar no Brasil.");
        RuleFor(location => location.Longitude)
            .InclusiveBetween(BrazilBounds.MinLongitude, BrazilBounds.MaxLongitude).When(location => location.Longitude is not null)
            .WithMessage("O ponto precisa estar no Brasil.");
        RuleFor(location => location.CityIbgeCode)
            .NotNull().When(location => location.Latitude is null)
            .WithMessage("Use a sua localização ou escolha uma cidade.");
    }
}
