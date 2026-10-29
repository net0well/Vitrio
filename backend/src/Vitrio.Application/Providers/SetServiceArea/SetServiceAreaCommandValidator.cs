using FluentValidation;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.SetServiceArea;

internal sealed class SetServiceAreaCommandValidator : AbstractValidator<SetServiceAreaCommand>
{
    public SetServiceAreaCommandValidator()
    {
        RuleFor(command => command.Cep)
            .Must(cep => Cep.Create(cep).IsSuccess).WithMessage(AccountErrors.InvalidCep.Message);
        RuleFor(command => command.Number)
            .NotEmpty().WithMessage("Informe o número.")
            .MaximumLength(BaseAddress.NumberMaxLength);
        RuleFor(command => command.Complement).MaximumLength(BaseAddress.ComplementMaxLength);
        RuleFor(command => command.RadiusKm)
            .InclusiveBetween(Provider.MinRadiusKm, Provider.MaxRadiusKm).WithMessage(ProviderErrors.InvalidRadius.Message);
        RuleFor(command => command.Neighborhoods)
            .Must(neighborhoods => neighborhoods.Count <= Provider.MaxNeighborhoods)
            .WithMessage(ProviderErrors.InvalidNeighborhoods.Message);
    }
}
