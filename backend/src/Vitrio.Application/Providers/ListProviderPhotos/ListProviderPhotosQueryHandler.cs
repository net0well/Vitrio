using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListProviderPhotos;

public sealed class ListProviderPhotosQueryHandler(ICurrentUser currentUser, IProviderSearchableRepository providers)
    : IQueryHandler<ListProviderPhotosQuery, Result<IReadOnlyList<ProviderPhotoResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ProviderPhotoResponse>>> Handle(ListProviderPhotosQuery query, CancellationToken cancellationToken) =>
        await providers.ListOwnPhotosAsync(currentUser.Id, cancellationToken) is { } photos
            ? Result<IReadOnlyList<ProviderPhotoResponse>>.Success(photos)
            : ProviderErrors.NotFound;
}
