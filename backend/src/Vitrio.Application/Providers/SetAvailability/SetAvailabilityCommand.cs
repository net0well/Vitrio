using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.SetAvailability;

public sealed record SetAvailabilityCommand(IReadOnlyList<AvailabilityInput> Slots, bool HandlesEmergencies) : ICommand<Result>;
