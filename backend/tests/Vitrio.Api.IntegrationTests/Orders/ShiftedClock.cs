using Vitrio.Shared.Time;

namespace Vitrio.Api.IntegrationTests.Orders;

public sealed class ShiftedClock(TimeProvider time, TimeSpan offset) : IClock
{
    public DateTimeOffset UtcNow => time.GetUtcNow() + offset;
}
