namespace Vitrio.Shared.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
