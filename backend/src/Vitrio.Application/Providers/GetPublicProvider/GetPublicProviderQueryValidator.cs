using FluentValidation;
using Vitrio.Domain.Locations;

namespace Vitrio.Application.Providers.GetPublicProvider;

internal sealed class GetPublicProviderQueryValidator : AbstractValidator<GetPublicProviderQuery>
{
    public GetPublicProviderQueryValidator()
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
    }
}
