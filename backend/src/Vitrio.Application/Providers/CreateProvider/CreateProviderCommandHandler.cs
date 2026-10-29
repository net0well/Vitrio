using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.CreateProvider;

public sealed class CreateProviderCommandHandler(
    ICurrentUser currentUser,
    IUserSearchableRepository users,
    IProviderWritableRepository providers,
    IProviderSearchableRepository providerQueries,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<CreateProviderCommand, Result<ProviderPrivateResponse>>
{
    public async ValueTask<Result<ProviderPrivateResponse>> Handle(CreateProviderCommand command, CancellationToken cancellationToken)
    {
        if (await providerQueries.HasProfileAsync(currentUser.Id, cancellationToken))
        {
            return ProviderErrors.AlreadyExists;
        }

        var me = await users.GetMeAsync(currentUser.Id, cancellationToken);
        var phone = me?.PhoneNumber is { } stored ? PhoneNumber.FromStored(stored) : null;
        var provider = Provider.Create(currentUser.Id, DisplayName.Create(command.DisplayName).Value, command.Headline, phone, clock);
        if (provider.IsFailure)
        {
            return provider.Error;
        }

        providers.Add(provider.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return (await providerQueries.GetPrivateAsync(currentUser.Id, clock.UtcNow, cancellationToken))!;
    }
}
