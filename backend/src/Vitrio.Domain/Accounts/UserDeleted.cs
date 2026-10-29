using Vitrio.Domain.Common;

namespace Vitrio.Domain.Accounts;

[EventType("user.deleted", 1)]
public sealed record UserDeleted(Guid UserId) : DomainEvent;
