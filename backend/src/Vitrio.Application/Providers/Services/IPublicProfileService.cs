using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Providers.Services;

public interface IPublicProfileService
{
    Task<ProviderPublicSnapshot?> GetSnapshotAsync(Guid providerId, CancellationToken cancellationToken);
}
