using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CompleteForgottenOrders;

public sealed record CompleteForgottenOrdersCommand(int BatchSize) : ICommand<Result>;
