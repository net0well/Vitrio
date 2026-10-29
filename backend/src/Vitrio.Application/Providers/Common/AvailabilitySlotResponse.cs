namespace Vitrio.Application.Providers.Common;

public sealed record AvailabilitySlotResponse(DayOfWeek DayOfWeek, string Start, string End);
