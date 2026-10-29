using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.SetAvailability;

public sealed record SetAvailabilityCommand(IReadOnlyList<AvailabilityInput> Slots, bool HandlesEmergencies) : ICommand<Result>;
