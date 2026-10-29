using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListPublicPhotos;

public sealed record ListPublicPhotosQuery(Guid ProviderId, int Page) : IQuery<Result<PublicPhotosPageResponse>>
{
    public const int PageSize = 12;
    public const int MaxPage = 10;
}
