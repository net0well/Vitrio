namespace Vitrio.Domain.Providers;

public sealed class AvailabilitySlot
{
    public const int SlotMinutes = 30;
    public const int MinutesPerDay = 24 * 60;
    public const int MaxPerDay = 3;

    private AvailabilitySlot()
    {
    }

    public Guid Id { get; private set; }

    public Guid ProviderId { get; private set; }

    public DayOfWeek DayOfWeek { get; private set; }

    public int StartMinute { get; private set; }

    public int EndMinute { get; private set; }

    internal static bool IsValid(AvailabilityDraft draft) =>
        Enum.IsDefined(draft.DayOfWeek)
        && draft.StartMinute % SlotMinutes == 0
        && draft.EndMinute % SlotMinutes == 0
        && draft.StartMinute >= 0
        && draft.EndMinute <= MinutesPerDay
        && draft.StartMinute < draft.EndMinute;

    internal static AvailabilitySlot Create(Guid providerId, AvailabilityDraft draft) => new()
    {
        Id = Guid.CreateVersion7(),
        ProviderId = providerId,
        DayOfWeek = draft.DayOfWeek,
        StartMinute = draft.StartMinute,
        EndMinute = draft.EndMinute
    };
}
