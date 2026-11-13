using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ViewProviderOrder;

public sealed record ViewProviderOrderCommand(Guid OrderId) : ICommand<Result<OrderProviderView>>;
