using FluentValidation;

namespace Vitrio.Application.Orders.CancelCustomerOrder;

internal sealed class CancelCustomerOrderCommandValidator : AbstractValidator<CancelCustomerOrderCommand>
{
    public CancelCustomerOrderCommandValidator() =>
        RuleFor(command => command.Reason).NotNull().IsInEnum().WithMessage("Escolha o motivo do cancelamento.");
}
