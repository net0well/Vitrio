using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ListProviderPhotos;

public sealed record ListProviderPhotosQuery : IQuery<Result<IReadOnlyList<ProviderPhotoResponse>>>;
