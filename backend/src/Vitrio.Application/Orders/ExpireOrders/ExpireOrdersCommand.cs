using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ExpireOrders;

public sealed record ExpireOrdersCommand(int BatchSize) : ICommand<Result>;
