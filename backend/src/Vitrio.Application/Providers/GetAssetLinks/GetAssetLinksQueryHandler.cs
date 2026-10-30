using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.GetAssetLinks;

public sealed class GetAssetLinksQueryHandler(IPublicLinks links) : IQueryHandler<GetAssetLinksQuery, Result<IReadOnlyList<AssetLinkResponse>>>
{
    private const string HandleAllUrls = "delegate_permission/common.handle_all_urls";
    private const string AndroidApp = "android_app";

    public ValueTask<Result<IReadOnlyList<AssetLinkResponse>>> Handle(GetAssetLinksQuery query, CancellationToken cancellationToken) =>
        ValueTask.FromResult(Result<IReadOnlyList<AssetLinkResponse>>.Success(
            [new AssetLinkResponse([HandleAllUrls], new AssetLinkTarget(AndroidApp, links.AndroidPackage, links.AndroidCertificateFingerprints))]));
}
