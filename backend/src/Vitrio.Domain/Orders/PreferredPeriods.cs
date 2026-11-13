namespace Vitrio.Domain.Orders;

public static class PreferredPeriods
{
    public static TimeOnly StartOf(PreferredPeriod period) => period switch
    {
        PreferredPeriod.Morning => new TimeOnly(7, 0),
        PreferredPeriod.Afternoon => new TimeOnly(12, 0),
        PreferredPeriod.Evening => new TimeOnly(18, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
    };

    public static TimeOnly EndOf(PreferredPeriod period) => period switch
    {
        PreferredPeriod.Morning => new TimeOnly(12, 0),
        PreferredPeriod.Afternoon => new TimeOnly(18, 0),
        PreferredPeriod.Evening => new TimeOnly(22, 0),
        _ => throw new ArgumentOutOfRangeException(nameof(period), period, null)
    };
}
