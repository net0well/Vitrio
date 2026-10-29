using FluentValidation;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.SetAvailability;

internal sealed class SetAvailabilityCommandValidator : AbstractValidator<SetAvailabilityCommand>
{
    public SetAvailabilityCommandValidator()
    {
        RuleForEach(command => command.Slots).ChildRules(slot =>
        {
            slot.RuleFor(item => item.DayOfWeek).IsInEnum();
            slot.RuleFor(item => item.Start)
                .Must(start => ClockTime.Parse(start) is not null).WithMessage("Use o horário no formato 08:00.");
            slot.RuleFor(item => item.End)
                .Must(end => ClockTime.Parse(end) is not null).WithMessage("Use o horário no formato 18:00.");
        });
        RuleFor(command => command.Slots)
            .Must(slots => slots.Count <= 7 * AvailabilitySlot.MaxPerDay).WithMessage(ProviderErrors.InvalidAvailability.Message);
    }
}
