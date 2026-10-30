namespace Vitrio.Shared.Results;

public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
    Unprocessable,
    RateLimited,
    Unavailable,
    Failure
}
