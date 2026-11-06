using FluentValidation;

namespace Vitrio.Application.Orders.AcceptQuote;

internal sealed class AcceptQuoteCommandValidator : AbstractValidator<AcceptQuoteCommand>
{
    public AcceptQuoteCommandValidator() =>
        RuleFor(command => command.QuoteVersion).GreaterThan(0).WithMessage("Informe a versão do orçamento que você viu.");
}
