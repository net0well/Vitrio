namespace Vitrio.Application.Accounts.ListSessions;

public sealed record SessionResponse(
    Guid Id,
    string DeviceName,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt,
    bool Current);
