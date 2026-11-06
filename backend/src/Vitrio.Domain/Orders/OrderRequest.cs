namespace Vitrio.Domain.Orders;

public sealed record OrderRequest(
    Guid? ServiceId,
    string ServiceTitle,
    int CategoryId,
    string Title,
    string Description,
    DateOnly PreferredDay,
    PreferredPeriod PreferredPeriod,
    OrderAddress Address,
    OrderContact Contact,
    OrderPlace Place,
    IReadOnlyList<string> PhotoUploadKeys);
