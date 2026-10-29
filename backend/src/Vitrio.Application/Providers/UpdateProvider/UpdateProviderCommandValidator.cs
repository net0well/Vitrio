using FluentValidation;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.UpdateProvider;

internal sealed class UpdateProviderCommandValidator : AbstractValidator<UpdateProviderCommand>
{
    public UpdateProviderCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .Must(name => DisplayName.Create(name).IsSuccess).When(command => command.DisplayName is not null)
            .WithMessage("Informe o nome que os clientes vão ver, com 2 a 60 caracteres e sem contato.");
        RuleFor(command => command.Headline)
            .NotEmpty().MaximumLength(Provider.HeadlineMaxLength).When(command => command.Headline is not null)
            .WithMessage("Informe a profissão com até 60 caracteres.");
        RuleFor(command => command.About)
            .Must(about => FreeText.Normalize(about).Length is >= Provider.AboutMinLength and <= Provider.AboutMaxLength)
            .When(command => command.About is not null)
            .WithMessage(ProviderErrors.InvalidAbout.Message);
        RuleFor(command => command.About)
            .Must(about => !FreeText.ContainsContactInformation(about!)).When(command => command.About is not null)
            .WithMessage(ProviderErrors.ContactInText.Message);
        RuleFor(command => command.YearsOfExperience)
            .InclusiveBetween(0, Provider.MaxYearsOfExperience).When(command => command.YearsOfExperience is not null)
            .WithMessage(ProviderErrors.InvalidYearsOfExperience.Message);
        RuleFor(command => command.ContactPhone)
            .Must(phone => PhoneNumber.Create(phone).IsSuccess).When(command => command.ContactPhone is not null)
            .WithMessage(AccountErrors.InvalidPhoneNumber.Message);
    }
}
