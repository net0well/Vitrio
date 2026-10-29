using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Providers.Repositories;

public interface IProviderSearchableRepository
{
    Task<bool> HasProfileAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> HasActiveProfileAsync(Guid userId, CancellationToken cancellationToken);

    Task<ProviderPrivateResponse?> GetPrivateAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<ProviderDashboardResponse?> GetDashboardAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);
}
