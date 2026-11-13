namespace Vitrio.Application.Orders.Common;

public sealed record OrderStoredPhotos(bool Removed, IReadOnlyList<string> ObjectKeys);
