using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.SetAvailability;

public sealed class SetAvailabilityCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetAvailabilityCommand, Result>
{
    public async ValueTask<Result> Handle(SetAvailabilityCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var drafts = command.Slots
            .Select(slot => new AvailabilityDraft(slot.DayOfWeek, ClockTime.Parse(slot.Start)!.Value, ClockTime.Parse(slot.End)!.Value))
            .ToList();
        var result = provider.SetAvailability(drafts, command.HandlesEmergencies, clock);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
