using System.Diagnostics.CodeAnalysis;

namespace Vitrio.Shared.Results;

public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess == error is not null)
        {
            throw new ArgumentException("A successful result has no error and a failed result requires one.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public Error? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
}
