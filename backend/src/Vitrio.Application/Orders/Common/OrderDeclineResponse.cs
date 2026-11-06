using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record OrderDeclineResponse(DeclineReason Reason, string? Details);
