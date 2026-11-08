using FluentValidation;

namespace Vitrio.Application.Orders.CancelProviderOrder;

internal sealed class CancelProviderOrderCommandValidator : AbstractValidator<CancelProviderOrderCommand>
{
    public CancelProviderOrderCommandValidator() =>
        RuleFor(command => command.Reason).NotNull().IsInEnum().WithMessage("Escolha o motivo do cancelamento.");
}
