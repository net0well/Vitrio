namespace Vitrio.Application.Providers.Common;

public sealed record PublicPhotoResponse(Guid Id, string Url, string ThumbnailUrl, string? Caption);
