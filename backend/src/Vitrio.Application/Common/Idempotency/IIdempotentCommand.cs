namespace Vitrio.Application.Common.Idempotency;

public interface IIdempotentCommand
{
    Guid IdempotencyKey { get; }

    string RequestHash { get; }
}
