using FluentValidation;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.SetProviderServices;

internal sealed class SetProviderServicesCommandValidator : AbstractValidator<SetProviderServicesCommand>
{
    public SetProviderServicesCommandValidator()
    {
        RuleFor(command => command.Services)
            .Must(services => services.Count <= Provider.MaxServices).WithMessage(ProviderErrors.InvalidServices.Message);
        RuleForEach(command => command.Services).ChildRules(service =>
        {
            service.RuleFor(item => item.Title)
                .NotEmpty().WithMessage("Dê um nome ao serviço.")
                .MaximumLength(ProviderService.TitleMaxLength);
            service.RuleFor(item => item.Description).MaximumLength(ProviderService.DescriptionMaxLength);
            service.RuleFor(item => item.PriceFrom)
                .NotNull().When(item => !item.OnQuote).WithMessage(ProviderErrors.InvalidPrice.Message)
                .InclusiveBetween(ProviderService.MinPriceCents / 100m, ProviderService.MaxPriceCents / 100m)
                .When(item => item.PriceFrom is not null).WithMessage(ProviderErrors.InvalidPrice.Message);
        });
    }
}
