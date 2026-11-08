using FluentValidation;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Orders.Common;

namespace Vitrio.Application.Orders.SendQuote;

internal sealed class SendQuoteCommandValidator : AbstractValidator<SendQuoteCommand>
{
    public SendQuoteCommandValidator()
    {
        RuleFor(command => command.IdempotencyKey).ValidIdempotencyKey();
        RuleFor(command => command.Type).NotNull().IsInEnum().WithMessage("Escolha Valor fechado ou Visita técnica.");
        RuleFor(command => command.Amount).NotNull().WithMessage("Informe o valor.");
        RuleFor(command => command.Date).NotNull().WithMessage("Escolha o dia.");
        RuleFor(command => command.Start).Must(start => OrderTimes.ParseStart(start) is not null).WithMessage("Escolha o início no formato HH:mm.");
        RuleFor(command => command.DurationMinutes).NotNull().WithMessage("Escolha a duração.");
    }
}
