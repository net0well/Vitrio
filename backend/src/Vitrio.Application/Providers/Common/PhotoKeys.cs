namespace Vitrio.Application.Providers.Common;

public static class PhotoKeys
{
    public const string WebpContentType = "image/webp";
    public const long MaxUploadBytes = 10 * 1024 * 1024;

    public static readonly IReadOnlySet<string> AcceptedContentTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public static string NewUpload(Guid providerId) => $"{providerId}/{Guid.CreateVersion7():N}";

    public static bool BelongsTo(string? uploadKey, Guid providerId) =>
        uploadKey is not null && uploadKey.StartsWith($"{providerId}/", StringComparison.Ordinal) && !uploadKey.Contains("..", StringComparison.Ordinal);

    public static string Full(Guid providerId, Guid photoId) => $"providers/{providerId}/{photoId:N}.webp";

    public static string Thumbnail(Guid providerId, Guid photoId) => $"providers/{providerId}/{photoId:N}-480.webp";

    public static string Avatar(Guid providerId) => $"providers/{providerId}/avatar-{Guid.CreateVersion7():N}.webp";
}
