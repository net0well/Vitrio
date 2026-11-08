namespace Vitrio.Application.Orders.Common;

public sealed record OrderProviderContext(DateTimeOffset CustomerSince, int CustomerOrderCount, double? DistanceKm);
