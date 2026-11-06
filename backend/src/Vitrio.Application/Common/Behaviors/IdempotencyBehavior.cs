using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Common.Repositories;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Common.Behaviors;

public sealed class IdempotencyBehavior<TMessage, TResponse>(
    ICurrentUser currentUser,
    IIdempotencyKeySearchableRepository storedKeys,
    IIdempotencyKeyWritableRepository keys,
    IDatabaseTransactionFactory transactions,
    IUnitOfWork unitOfWork,
    IClock clock) : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        if (message is not IIdempotentCommand command)
        {
            return await next(message, cancellationToken);
        }

        if (await storedKeys.FindAsync(currentUser.Id, command.IdempotencyKey, cancellationToken) is { } stored)
        {
            return Replay(stored, command);
        }

        var (response, lostTheRace) = await RunAndStoreAsync(message, command, next, cancellationToken);
        if (!lostTheRace)
        {
            return response;
        }

        var winner = await storedKeys.FindAsync(currentUser.Id, command.IdempotencyKey, cancellationToken)
            ?? throw new InvalidOperationException("The idempotency key was taken, but its response could not be found.");
        return Replay(winner, command);
    }

    private async Task<(TResponse Response, bool LostTheRace)> RunAndStoreAsync(
        TMessage message,
        IIdempotentCommand command,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        await using var transaction = await transactions.BeginAsync(cancellationToken);
        var response = await next(message, cancellationToken);
        if (response is not Result { IsSuccess: true })
        {
            return (response, false);
        }

        keys.Add(new IdempotencyKeyEntry(
            currentUser.Id,
            command.IdempotencyKey,
            command.RequestHash,
            IdempotentResponse<TResponse>.Serialize(response),
            clock.UtcNow));
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            return (response, true);
        }

        await transaction.CommitAsync(cancellationToken);
        return (response, false);
    }

    private static TResponse Replay(StoredIdempotentResponse stored, IIdempotentCommand command) =>
        string.Equals(stored.RequestHash, command.RequestHash, StringComparison.Ordinal)
            ? IdempotentResponse<TResponse>.Restore(stored.ResponseBody)
            : FailedResult<TResponse>.From(IdempotencyErrors.KeyReused);
}
