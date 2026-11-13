namespace Vitrio.Application.Orders.Common;

public sealed record OrderCustomerCard(string DisplayName, string? FullName, string? Phone, DateTimeOffset MemberSince, int OrderCount);
