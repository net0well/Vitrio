using System.Globalization;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Common;

public sealed record ServiceTimeResponse(DateOnly Date, string Start, string End, int DurationMinutes)
{
    public static ServiceTimeResponse From(ServiceSchedule schedule) => new(
        schedule.Date,
        schedule.Start.ToString("HH:mm", CultureInfo.InvariantCulture),
        schedule.End.ToString("HH:mm", CultureInfo.InvariantCulture),
        schedule.DurationMinutes);
}
