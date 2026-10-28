namespace Vitrio.Domain.Common;

public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(true, null) => _value = value;

    private Result(Error error)
        : base(false, error)
    {
    }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(error);

    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
}
