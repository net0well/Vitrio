using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CompleteProviderOrder;

public sealed record CompleteProviderOrderCommand(Guid OrderId) : ICommand<Result<OrderProviderView>>;
