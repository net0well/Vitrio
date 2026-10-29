using System.Globalization;

namespace Vitrio.Shared.Time;

public static class ClockTime
{
    private const int MinutesPerHour = 60;
    private const int MinutesPerDay = 24 * MinutesPerHour;

    public static int? Parse(string? value)
    {
        if (value is not { Length: 5 } || value[2] != ':'
            || !int.TryParse(value.AsSpan(0, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var hours)
            || !int.TryParse(value.AsSpan(3, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var minutes)
            || minutes >= MinutesPerHour)
        {
            return null;
        }

        var total = (hours * MinutesPerHour) + minutes;
        return total <= MinutesPerDay ? total : null;
    }

    public static string Format(int minuteOfDay) =>
        string.Create(CultureInfo.InvariantCulture, $"{minuteOfDay / MinutesPerHour:00}:{minuteOfDay % MinutesPerHour:00}");
}
