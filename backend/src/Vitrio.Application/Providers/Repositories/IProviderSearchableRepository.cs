using Vitrio.Application.Providers.Common;

namespace Vitrio.Application.Providers.Repositories;

public interface IProviderSearchableRepository
{
    Task<bool> HasProfileAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> HasActiveProfileAsync(Guid userId, CancellationToken cancellationToken);

    Task<ProviderPrivateResponse?> GetPrivateAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<ProviderDashboardResponse?> GetDashboardAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken);

    Task<ProviderPublicSnapshot?> GetPublicSnapshotAsync(Guid providerId, CancellationToken cancellationToken);

    Task<Guid?> FindIdByUserAsync(Guid userId, CancellationToken cancellationToken);

    Task<Guid?> FindPublishedOwnerAsync(Guid providerId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProviderPhotoResponse>?> ListOwnPhotosAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<PublicPhotoResponse>?> GetPublicPhotosAsync(Guid providerId, int skip, int take, CancellationToken cancellationToken);

    Task<IReadOnlyList<PendingPhoto>> ListPhotosProcessingSinceAsync(DateTimeOffset createdBefore, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<string>> ListPendingUploadKeysAsync(CancellationToken cancellationToken);

    Task<bool> IsCnpjInUseAsync(string cnpj, Guid exceptProviderId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> ListMeiDueAsync(
        DateTimeOffset pendingCheckedBefore,
        DateTimeOffset verifiedCheckedBefore,
        int take,
        CancellationToken cancellationToken);
}
