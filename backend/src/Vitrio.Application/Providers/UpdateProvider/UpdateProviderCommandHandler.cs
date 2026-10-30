using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.UpdateProvider;

public sealed class UpdateProviderCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IProviderSearchableRepository providerQueries,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<UpdateProviderCommand, Result<ProviderPrivateResponse>>
{
    public async ValueTask<Result<ProviderPrivateResponse>> Handle(UpdateProviderCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var changes = new ProfileChanges(
            command.DisplayName is null ? null : DisplayName.Create(command.DisplayName).Value,
            command.Headline,
            command.About,
            command.YearsOfExperience,
            command.ContactPhone is null ? null : PhoneNumber.Create(command.ContactPhone).Value,
            command.WhatsAppEnabled,
            command.HandlesEmergencies);
        var updated = provider.UpdateProfile(changes, clock);
        if (updated.IsFailure)
        {
            return updated.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (await providerQueries.GetPrivateAsync(currentUser.Id, clock.UtcNow, cancellationToken))!;
    }
}
