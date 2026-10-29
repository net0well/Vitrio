using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.SetProviderServices;

public sealed class SetProviderServicesCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetProviderServicesCommand, Result>
{
    public async ValueTask<Result> Handle(SetProviderServicesCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var drafts = command.Services
            .Select(service => new ServiceDraft(
                service.Title,
                service.Description,
                Money.FromReais(service.PriceFrom),
                service.PriceUnit ?? PriceUnit.PerService,
                service.OnQuote))
            .ToList();
        var result = provider.SetServices(drafts, clock);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
