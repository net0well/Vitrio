using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.Events;

public sealed class RemoveProviderProfileOfDeletedUser(IProviderWritableRepository providers, IUnitOfWork unitOfWork, IClock clock)
    : IDomainEventHandler<UserDeleted>
{
    public async ValueTask Handle(UserDeleted domainEvent, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(domainEvent.UserId, cancellationToken);
        if (provider is null)
        {
            return;
        }

        provider.Remove(clock);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
