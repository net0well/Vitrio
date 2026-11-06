namespace Vitrio.Domain.Common;

public static class CityTime
{
    public static TimeZoneInfo Zone(string timeZoneId) => TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

    public static DateTimeOffset ToUtc(DateOnly date, TimeOnly time, TimeZoneInfo zone)
    {
        var local = date.ToDateTime(time, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, zone.GetUtcOffset(local)).ToUniversalTime();
    }

    public static DateTimeOffset ToLocal(DateTimeOffset instant, TimeZoneInfo zone) => TimeZoneInfo.ConvertTime(instant, zone);

    public static DateOnly Today(DateTimeOffset now, TimeZoneInfo zone) => DateOnly.FromDateTime(ToLocal(now, zone).DateTime);
}
