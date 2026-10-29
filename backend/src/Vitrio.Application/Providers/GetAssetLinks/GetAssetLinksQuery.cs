using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.GetAssetLinks;

public sealed record GetAssetLinksQuery : IQuery<Result<IReadOnlyList<AssetLinkResponse>>>;
