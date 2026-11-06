using Mediator;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Orders.Common;
using Vitrio.Domain.Orders;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CreateOrder;

public sealed record CreateOrderCommand(
    Guid ProviderId,
    Guid? ServiceId,
    string Title,
    string Description,
    DateOnly PreferredDay,
    PreferredPeriod? PreferredPeriod,
    Guid AddressId,
    IReadOnlyList<string> PhotoKeys,
    Guid IdempotencyKey,
    string RequestHash) : ICommand<Result<OrderCustomerView>>, IIdempotentCommand;
