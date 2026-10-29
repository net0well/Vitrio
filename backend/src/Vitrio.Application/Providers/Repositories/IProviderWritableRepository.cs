using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Repositories;

public interface IProviderWritableRepository
{
    Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Provider provider);
}
