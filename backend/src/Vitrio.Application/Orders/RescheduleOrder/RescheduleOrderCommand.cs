using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.RescheduleOrder;

public sealed record RescheduleOrderCommand(Guid OrderId, DateOnly? Date, string? Start, int? DurationMinutes) : ICommand<Result<OrderProviderView>>;
