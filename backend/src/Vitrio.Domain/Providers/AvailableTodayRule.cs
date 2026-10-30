namespace Vitrio.Domain.Providers;

public static class AvailableTodayRule
{
    public static bool IsOn(bool availableToday, DateTimeOffset? setAt, DateTimeOffset now, string? timeZoneId)
    {
        if (!availableToday || setAt is not { } since || timeZoneId is null)
        {
            return false;
        }

        var zone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(since, zone).DateTime)
            == DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, zone).DateTime);
    }
}
