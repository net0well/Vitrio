using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Repositories;

public interface IProviderWritableRepository
{
    Task<Provider?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken);

    void Add(Provider provider);
}
