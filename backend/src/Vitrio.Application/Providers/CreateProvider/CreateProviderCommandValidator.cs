using FluentValidation;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.CreateProvider;

internal sealed class CreateProviderCommandValidator : AbstractValidator<CreateProviderCommand>
{
    public CreateProviderCommandValidator()
    {
        RuleFor(command => command.DisplayName)
            .Must(name => DisplayName.Create(name).IsSuccess)
            .WithMessage("Informe o nome que os clientes vão ver, com 2 a 60 caracteres e sem contato.");
        RuleFor(command => command.Headline)
            .NotEmpty().WithMessage("Informe a sua profissão.")
            .MaximumLength(Provider.HeadlineMaxLength);
    }
}
