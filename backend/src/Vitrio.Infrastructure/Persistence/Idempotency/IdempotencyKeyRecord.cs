namespace Vitrio.Infrastructure.Persistence.Idempotency;

public sealed class IdempotencyKeyRecord
{
    private IdempotencyKeyRecord()
    {
    }

    public Guid UserId { get; private set; }

    public Guid Key { get; private set; }

    public string RequestHash { get; private set; } = string.Empty;

    public string ResponseBody { get; private set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; private set; }

    public static IdempotencyKeyRecord Create(Guid userId, Guid key, string requestHash, string responseBody, DateTimeOffset createdAt) => new()
    {
        UserId = userId,
        Key = key,
        RequestHash = requestHash,
        ResponseBody = responseBody,
        CreatedAt = createdAt
    };
}
