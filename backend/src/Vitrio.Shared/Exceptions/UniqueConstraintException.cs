namespace Vitrio.Shared.Exceptions;

public sealed class UniqueConstraintException : Exception
{
    public UniqueConstraintException()
        : base("The data already exists.")
    {
    }

    public UniqueConstraintException(string message)
        : base(message)
    {
    }

    public UniqueConstraintException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
