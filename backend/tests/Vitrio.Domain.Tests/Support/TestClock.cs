using Vitrio.Shared.Time;

namespace Vitrio.Domain.Tests.Support;

public sealed class TestClock(DateTimeOffset now) : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = now;

    public void Advance(TimeSpan by) => UtcNow += by;

    public void Set(DateTimeOffset now) => UtcNow = now;
}
