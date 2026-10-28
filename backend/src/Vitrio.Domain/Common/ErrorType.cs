namespace Vitrio.Domain.Common;

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
