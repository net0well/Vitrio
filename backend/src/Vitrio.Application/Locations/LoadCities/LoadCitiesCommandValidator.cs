using FluentValidation;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Locations.LoadCities;

internal sealed class LoadCitiesCommandValidator : AbstractValidator<LoadCitiesCommand>
{
    public LoadCitiesCommandValidator()
    {
        RuleForEach(command => command.States)
            .Must(BrazilStates.IsState)
            .WithMessage("Informe cada UF com 2 letras.");
    }
}
