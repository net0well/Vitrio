using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.GetProviderAgenda;

public sealed record GetProviderAgendaQuery(DateOnly? From, DateOnly? To) : IQuery<Result<IReadOnlyList<AgendaItemResponse>>>
{
    public const int MaxDays = 42;
}
