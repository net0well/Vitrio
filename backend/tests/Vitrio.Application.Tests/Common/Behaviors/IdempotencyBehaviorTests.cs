using Mediator;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common.Behaviors;
using Vitrio.Application.Common.Idempotency;
using Vitrio.Application.Common.Repositories;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Tests.Common.Behaviors;

public sealed class IdempotencyBehaviorTests
{
    private static readonly Guid UserId = Guid.Parse("0193c9a8-0000-7000-8000-0000000000aa");
    private static readonly Guid Key = Guid.Parse("0193c9a8-0000-7000-8000-0000000000bb");
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 15, 0, 0, TimeSpan.Zero);

    private readonly ICurrentUser _currentUser = Substitute.For<ICurrentUser>();
    private readonly IIdempotencyKeySearchableRepository _storedKeys = Substitute.For<IIdempotencyKeySearchableRepository>();
    private readonly IIdempotencyKeyWritableRepository _keys = Substitute.For<IIdempotencyKeyWritableRepository>();
    private readonly IDatabaseTransactionFactory _transactions = Substitute.For<IDatabaseTransactionFactory>();
    private readonly IDatabaseTransaction _transaction = Substitute.For<IDatabaseTransaction>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IClock _clock = Substitute.For<IClock>();

    public IdempotencyBehaviorTests()
    {
        _currentUser.Id.Returns(UserId);
        _clock.UtcNow.Returns(Now);
        _transactions.BeginAsync(Arg.Any<CancellationToken>()).Returns(_transaction);
    }

    [Fact]
    public async Task Handle_WithANewKey_RunsTheCommandAndStoresTheResponseInTheSameTransaction()
    {
        var created = new SampleView(Guid.CreateVersion7(), "Chuveiro", 180.5m);

        var result = await Behavior<CreateSample, Result<SampleView>>().Handle(
            new CreateSample(Key, "hash-1"),
            (_, _) => ValueTask.FromResult<Result<SampleView>>(created),
            TestContext.Current.CancellationToken);

        Assert.Equal(created, result.Value);
        _keys.Received(1).Add(Arg.Is<IdempotencyKeyEntry>(entry =>
            entry.UserId == UserId && entry.Key == Key && entry.RequestHash == "hash-1" && entry.CreatedAt == Now
            && entry.ResponseBody.Contains("Chuveiro", StringComparison.Ordinal)));
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTheSameKeyAndBody_ReturnsTheStoredResponseWithoutRunningAgain()
    {
        var stored = new SampleView(Guid.CreateVersion7(), "Chuveiro", 180.5m);
        _storedKeys.FindAsync(UserId, Key, Arg.Any<CancellationToken>())
            .Returns(new StoredIdempotentResponse("hash-1", IdempotentResponse<Result<SampleView>>.Serialize(stored)));
        var nextWasCalled = false;

        var result = await Behavior<CreateSample, Result<SampleView>>().Handle(
            new CreateSample(Key, "hash-1"),
            (_, _) =>
            {
                nextWasCalled = true;
                return ValueTask.FromResult<Result<SampleView>>(new SampleView(Guid.Empty, "Outro", 1m));
            },
            TestContext.Current.CancellationToken);

        Assert.False(nextWasCalled);
        Assert.Equal(stored, result.Value);
        await _transactions.DidNotReceive().BeginAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTheSameKeyAndADifferentBody_ReturnsKeyReused()
    {
        _storedKeys.FindAsync(UserId, Key, Arg.Any<CancellationToken>())
            .Returns(new StoredIdempotentResponse("hash-1", "{}"));

        var result = await Behavior<CreateSample, Result<SampleView>>().Handle(
            new CreateSample(Key, "hash-2"),
            (_, _) => ValueTask.FromResult<Result<SampleView>>(new SampleView(Guid.Empty, "Outro", 1m)),
            TestContext.Current.CancellationToken);

        Assert.Equal(IdempotencyErrors.KeyReused, result.Error);
        Assert.Equal(ErrorType.Unprocessable, result.Error!.Type);
    }

    [Fact]
    public async Task Handle_WhenTheCommandFails_StoresNothingAndDoesNotCommit()
    {
        var failure = Error.Conflict("sample.conflict", "Não deu.");

        var result = await Behavior<CreateSample, Result<SampleView>>().Handle(
            new CreateSample(Key, "hash-1"),
            (_, _) => ValueTask.FromResult<Result<SampleView>>(failure),
            TestContext.Current.CancellationToken);

        Assert.Equal(failure, result.Error);
        _keys.DidNotReceive().Add(Arg.Any<IdempotencyKeyEntry>());
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _transaction.Received(1).DisposeAsync();
    }

    [Fact]
    public async Task Handle_WhenAnotherRequestStoredTheKeyFirst_ReturnsThatResponse()
    {
        var first = new SampleView(Guid.CreateVersion7(), "Primeiro", 10m);
        _storedKeys.FindAsync(UserId, Key, Arg.Any<CancellationToken>())
            .Returns(null, new StoredIdempotentResponse("hash-1", IdempotentResponse<Result<SampleView>>.Serialize(first)));
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new UniqueConstraintException("duplicate", new InvalidOperationException()));

        var result = await Behavior<CreateSample, Result<SampleView>>().Handle(
            new CreateSample(Key, "hash-1"),
            (_, _) => ValueTask.FromResult<Result<SampleView>>(new SampleView(Guid.CreateVersion7(), "Segundo", 20m)),
            TestContext.Current.CancellationToken);

        Assert.Equal(first, result.Value);
        await _transaction.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTheMessageIsNotIdempotent_JustRunsIt()
    {
        var result = await Behavior<RemoveSample, Result>().Handle(
            new RemoveSample(Guid.CreateVersion7()),
            (_, _) => ValueTask.FromResult(Result.Success()),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        await _storedKeys.DidNotReceive().FindAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _transactions.DidNotReceive().BeginAsync(Arg.Any<CancellationToken>());
    }

    private IdempotencyBehavior<TMessage, TResponse> Behavior<TMessage, TResponse>()
        where TMessage : IMessage =>
        new(_currentUser, _storedKeys, _keys, _transactions, _unitOfWork, _clock);

    public sealed record SampleView(Guid Id, string Title, decimal Amount);

    public sealed record CreateSample(Guid IdempotencyKey, string RequestHash) : ICommand<Result<SampleView>>, IIdempotentCommand;

    public sealed record RemoveSample(Guid Id) : ICommand<Result>;
}
