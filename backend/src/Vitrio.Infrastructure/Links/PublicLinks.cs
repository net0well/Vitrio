using Microsoft.Extensions.Options;
using Vitrio.Application.Abstractions;

namespace Vitrio.Infrastructure.Links;

internal sealed class PublicLinks(IOptions<PublicLinksOptions> options) : IPublicLinks
{
    public string ProviderProfile(Guid providerId) => $"{options.Value.BaseUrl.TrimEnd('/')}/p/{providerId}";
}
