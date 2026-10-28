namespace Vitrio.Domain.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
