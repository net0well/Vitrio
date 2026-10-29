namespace Vitrio.Domain.Common;

public record Error(string Code, string Message, ErrorType Type)
{
    public TimeSpan? RetryAfter { get; init; }

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);

    public static Error Unprocessable(string code, string message) => new(code, message, ErrorType.Unprocessable);

    public static Error RateLimited(string code, string message) => new(code, message, ErrorType.RateLimited);

    public static Error Unavailable(string code, string message) => new(code, message, ErrorType.Unavailable);

    public static Error Failure(string code, string message) => new(code, message, ErrorType.Failure);
}
