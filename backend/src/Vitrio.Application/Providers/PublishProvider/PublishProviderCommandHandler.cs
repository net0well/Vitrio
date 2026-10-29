using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.PublishProvider;

public sealed class PublishProviderCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<PublishProviderCommand, Result<ProviderStatusResponse>>
{
    public async ValueTask<Result<ProviderStatusResponse>> Handle(PublishProviderCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var result = provider.Publish(clock);
        if (result.IsFailure)
        {
            return result.Error;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ProviderStatusResponse(provider.Status);
    }
}
