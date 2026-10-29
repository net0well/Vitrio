using FluentValidation;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Locations.LocateCity;

internal sealed class LocateCityQueryValidator : AbstractValidator<LocateCityQuery>
{
    public LocateCityQueryValidator()
    {
        RuleFor(query => query.Latitude)
            .NotNull().WithMessage("Informe a latitude.")
            .InclusiveBetween(BrazilBounds.MinLatitude, BrazilBounds.MaxLatitude).WithMessage("O ponto precisa estar no Brasil.");
        RuleFor(query => query.Longitude)
            .NotNull().WithMessage("Informe a longitude.")
            .InclusiveBetween(BrazilBounds.MinLongitude, BrazilBounds.MaxLongitude).WithMessage("O ponto precisa estar no Brasil.");
    }
}
