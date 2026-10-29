namespace Vitrio.Shared.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException()
        : base("The data was changed by another request.")
    {
    }

    public ConcurrencyConflictException(string message)
        : base(message)
    {
    }

    public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
