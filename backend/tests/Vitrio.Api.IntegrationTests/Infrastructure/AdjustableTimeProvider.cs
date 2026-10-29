namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class AdjustableTimeProvider : TimeProvider
{
    private long _offsetTicks;

    public override DateTimeOffset GetUtcNow() => System.GetUtcNow().AddTicks(Interlocked.Read(ref _offsetTicks));

    public void Advance(TimeSpan duration) => Interlocked.Add(ref _offsetTicks, duration.Ticks);
}
