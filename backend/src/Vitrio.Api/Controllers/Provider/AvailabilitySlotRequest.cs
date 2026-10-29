using Vitrio.Application.Providers.SetAvailability;

namespace Vitrio.Api.Controllers.Provider;

public sealed record AvailabilitySlotRequest(DayOfWeek? DayOfWeek, string? Start, string? End)
{
    public AvailabilityInput ToInput() => new(DayOfWeek ?? (DayOfWeek)(-1), Start, End);
}
