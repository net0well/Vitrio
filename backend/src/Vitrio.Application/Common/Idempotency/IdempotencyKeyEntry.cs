namespace Vitrio.Application.Common.Idempotency;

public sealed record IdempotencyKeyEntry(Guid UserId, Guid Key, string RequestHash, string ResponseBody, DateTimeOffset CreatedAt);
