using Microsoft.Extensions.Options;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Links;

internal sealed class PublicLinks(IOptions<PublicLinksOptions> options) : IPublicLinks
{
    public string PlayStore => options.Value.PlayStoreUrl;

    public string AndroidPackage => options.Value.AndroidPackage;

    public IReadOnlyList<string> AndroidCertificateFingerprints => options.Value.AndroidCertificateFingerprints;

    public string ProviderProfile(Guid providerId) => $"{options.Value.BaseUrl.TrimEnd('/')}/p/{providerId}";
}
