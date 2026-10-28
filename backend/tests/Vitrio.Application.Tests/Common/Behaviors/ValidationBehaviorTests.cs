using FluentValidation;
using Mediator;
using Vitrio.Application.Common.Behaviors;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    private readonly CreateSampleValidator _validator = new();

    [Fact]
    public async Task Handle_WhenMessageIsValid_CallsTheNextStep()
    {
        var behavior = new ValidationBehavior<CreateSample, Result<Guid>>([_validator]);
        var expectedId = Guid.CreateVersion7();

        var result = await behavior.Handle(
            new CreateSample("Pintura", 2),
            (_, _) => ValueTask.FromResult<Result<Guid>>(expectedId),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedId, result.Value);
    }

    [Fact]
    public async Task Handle_WhenMessageIsInvalid_ReturnsFieldErrorsWithoutCallingTheNextStep()
    {
        var behavior = new ValidationBehavior<CreateSample, Result<Guid>>([_validator]);
        var nextWasCalled = false;

        var result = await behavior.Handle(
            new CreateSample("", 0),
            (_, _) =>
            {
                nextWasCalled = true;
                return ValueTask.FromResult<Result<Guid>>(Guid.Empty);
            },
            TestContext.Current.CancellationToken);

        Assert.False(nextWasCalled);
        Assert.True(result.IsFailure);
        var error = Assert.IsType<ValidationError>(result.Error);
        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.Equal(["Name", "Quantity"], error.Errors.Keys.Order());
    }

    [Fact]
    public async Task Handle_WhenResponseIsNonGenericResult_ReturnsFailure()
    {
        var behavior = new ValidationBehavior<RemoveSample, Result>([new RemoveSampleValidator()]);

        var result = await behavior.Handle(
            new RemoveSample(Guid.Empty),
            (_, _) => ValueTask.FromResult(Result.Success()),
            TestContext.Current.CancellationToken);

        Assert.IsType<ValidationError>(result.Error);
    }

    [Fact]
    public async Task Handle_WhenThereAreNoValidators_CallsTheNextStep()
    {
        var behavior = new ValidationBehavior<CreateSample, Result<Guid>>([]);

        var result = await behavior.Handle(
            new CreateSample("", 0),
            (_, _) => ValueTask.FromResult<Result<Guid>>(Guid.Empty),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_WhenInvalidMessageDoesNotReturnResult_Throws()
    {
        var behavior = new ValidationBehavior<CountSamples, int>([new CountSamplesValidator()]);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await behavior.Handle(
            new CountSamples(-1),
            (_, _) => ValueTask.FromResult(0),
            TestContext.Current.CancellationToken));
    }

    public sealed record CreateSample(string Name, int Quantity) : ICommand<Result<Guid>>;

    public sealed record RemoveSample(Guid Id) : ICommand<Result>;

    public sealed record CountSamples(int Minimum) : IQuery<int>;

    private sealed class CreateSampleValidator : AbstractValidator<CreateSample>
    {
        public CreateSampleValidator()
        {
            RuleFor(command => command.Name).NotEmpty();
            RuleFor(command => command.Quantity).GreaterThan(0);
        }
    }

    private sealed class RemoveSampleValidator : AbstractValidator<RemoveSample>
    {
        public RemoveSampleValidator() => RuleFor(command => command.Id).NotEmpty();
    }

    private sealed class CountSamplesValidator : AbstractValidator<CountSamples>
    {
        public CountSamplesValidator() => RuleFor(query => query.Minimum).GreaterThanOrEqualTo(0);
    }
}
