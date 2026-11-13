using Vitrio.Application.Orders.CreateOrder;
using Vitrio.Domain.Orders;

namespace Vitrio.Api.Controllers.Orders;

public sealed record CreateOrderRequest(
    Guid? ProviderId,
    Guid? ServiceId,
    string? Title,
    string? Description,
    DateOnly? PreferredDay,
    PreferredPeriod? PreferredPeriod,
    Guid? AddressId,
    IReadOnlyList<string>? PhotoKeys)
{
    public CreateOrderCommand ToCommand(Guid? idempotencyKey, string requestHash) => new(
        ProviderId ?? Guid.Empty,
        ServiceId,
        Title ?? string.Empty,
        Description ?? string.Empty,
        PreferredDay ?? default,
        PreferredPeriod,
        AddressId ?? Guid.Empty,
        PhotoKeys ?? [],
        idempotencyKey ?? Guid.Empty,
        requestHash);
}
