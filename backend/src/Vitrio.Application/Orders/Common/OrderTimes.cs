using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.Common;

public static class OrderTimes
{
    private const int MinutesPerDay = 24 * 60;

    public static TimeOnly? ParseStart(string? value) =>
        ClockTime.Parse(value) is { } minutes and < MinutesPerDay ? TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(minutes)) : null;
}
