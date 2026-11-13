using FluentValidation;

namespace Vitrio.Application.Orders.DeclineOrder;

internal sealed class DeclineOrderCommandValidator : AbstractValidator<DeclineOrderCommand>
{
    public DeclineOrderCommandValidator() =>
        RuleFor(command => command.Reason).NotNull().IsInEnum().WithMessage("Escolha o motivo da recusa.");
}
