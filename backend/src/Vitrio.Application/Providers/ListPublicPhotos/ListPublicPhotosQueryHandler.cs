using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListPublicPhotos;

public sealed class ListPublicPhotosQueryHandler(IProviderSearchableRepository providers)
    : IQueryHandler<ListPublicPhotosQuery, Result<PublicPhotosPageResponse>>
{
    public async ValueTask<Result<PublicPhotosPageResponse>> Handle(ListPublicPhotosQuery query, CancellationToken cancellationToken)
    {
        var photos = await providers.GetPublicPhotosAsync(
            query.ProviderId,
            (query.Page - 1) * ListPublicPhotosQuery.PageSize,
            ListPublicPhotosQuery.PageSize + 1,
            cancellationToken);
        if (photos is null)
        {
            return ProviderErrors.NotFound;
        }

        return new PublicPhotosPageResponse(photos.Take(ListPublicPhotosQuery.PageSize).ToList(), photos.Count > ListPublicPhotosQuery.PageSize);
    }
}
