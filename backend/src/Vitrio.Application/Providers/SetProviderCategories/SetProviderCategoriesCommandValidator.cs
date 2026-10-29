using FluentValidation;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.SetProviderCategories;

internal sealed class SetProviderCategoriesCommandValidator : AbstractValidator<SetProviderCategoriesCommand>
{
    public SetProviderCategoriesCommandValidator()
    {
        RuleFor(command => command.CategoryIds)
            .Must(ids => ids.Count is > 0 and <= Provider.MaxCategories && ids.Distinct().Count() == ids.Count)
            .WithMessage(ProviderErrors.InvalidCategories.Message);
        RuleFor(command => command.PrimaryCategoryId)
            .Must((command, primary) => command.CategoryIds.Contains(primary))
            .WithMessage("Marque uma das categorias escolhidas como principal.");
    }
}
