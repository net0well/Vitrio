using FluentValidation;
using Vitrio.Application.Orders.Common;

namespace Vitrio.Application.Orders.RescheduleOrder;

internal sealed class RescheduleOrderCommandValidator : AbstractValidator<RescheduleOrderCommand>
{
    public RescheduleOrderCommandValidator()
    {
        RuleFor(command => command.Date).NotNull().WithMessage("Escolha o dia.");
        RuleFor(command => command.Start).Must(start => OrderTimes.ParseStart(start) is not null).WithMessage("Escolha o início no formato HH:mm.");
        RuleFor(command => command.DurationMinutes).NotNull().WithMessage("Escolha a duração.");
    }
}
