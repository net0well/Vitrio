using Microsoft.Extensions.Options;

namespace Vitrio.Infrastructure.Storage;

internal sealed class PublicMedia(IOptions<StorageOptions> options)
{
    public string Url(string key) => $"{options.Value.PublicBaseUrl.TrimEnd('/')}/{key}";

    public string? UrlOrNull(string? key) => key is null ? null : Url(key);
}
