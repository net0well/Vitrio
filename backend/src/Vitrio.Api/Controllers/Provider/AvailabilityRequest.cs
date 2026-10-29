using Vitrio.Application.Providers.SetAvailability;

namespace Vitrio.Api.Controllers.Provider;

public sealed record AvailabilityRequest(IReadOnlyList<AvailabilitySlotRequest>? Slots, bool? HandlesEmergencies)
{
    public SetAvailabilityCommand ToCommand() =>
        new((Slots ?? []).Select(slot => slot.ToInput()).ToList(), HandlesEmergencies ?? false);
}
