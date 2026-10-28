using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Common;

public sealed class ResultTests
{
    private static readonly Error SampleError = Error.Conflict("sample.conflict", "Conflito de exemplo.");

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_KeepsTheError()
    {
        var result = Result.Failure(SampleError);

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void Failure_WithoutError_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(null!));
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result result = SampleError;

        Assert.True(result.IsFailure);
        Assert.Equal(SampleError, result.Error);
    }

    [Fact]
    public void GenericResult_FromValue_ExposesTheValue()
    {
        Result<int> result = 42;

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericResult_FromError_ThrowsWhenReadingTheValue()
    {
        Result<int> result = SampleError;

        Assert.True(result.IsFailure);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void GenericFailure_ReturnsTheGenericType()
    {
        var result = Result<string>.Failure(SampleError);

        Assert.IsType<Result<string>>(result);
        Assert.Equal(SampleError, result.Error);
    }
}
