using FluentValidation;

namespace Vitrio.Application.Orders.GetProviderAgenda;

internal sealed class GetProviderAgendaQueryValidator : AbstractValidator<GetProviderAgendaQuery>
{
    public GetProviderAgendaQueryValidator()
    {
        RuleFor(query => query.From).NotNull().WithMessage("Informe o primeiro dia.");
        RuleFor(query => query.To).NotNull().WithMessage("Informe o último dia.");
        RuleFor(query => query)
            .Must(query => query.From is not { } from || query.To is not { } to || (to >= from && to.DayNumber - from.DayNumber < GetProviderAgendaQuery.MaxDays))
            .WithName("to")
            .WithMessage("Peça até 42 dias, com o último dia depois do primeiro.");
    }
}
