using Mediator;
using Vitrio.Application.Common.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Common.DeleteExpiredIdempotencyKeys;

public sealed class DeleteExpiredIdempotencyKeysCommandHandler(IIdempotencyKeyWritableRepository keys, IClock clock)
    : ICommandHandler<DeleteExpiredIdempotencyKeysCommand, Result>
{
    private static readonly TimeSpan KeptFor = TimeSpan.FromHours(24);

    public async ValueTask<Result> Handle(DeleteExpiredIdempotencyKeysCommand command, CancellationToken cancellationToken)
    {
        await keys.DeleteCreatedBeforeAsync(clock.UtcNow - KeptFor, cancellationToken);
        return Result.Success();
    }
}
