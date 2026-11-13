using Vitrio.Domain.Common;

namespace Vitrio.Application.Orders.Common;

public static class CityInstant
{
    public static DateTimeOffset In(DateTimeOffset instant, string timeZoneId) => CityTime.ToLocal(instant, CityTime.Zone(timeZoneId));

    public static DateTimeOffset? In(DateTimeOffset? instant, string timeZoneId) => instant is { } value ? In(value, timeZoneId) : null;
}
