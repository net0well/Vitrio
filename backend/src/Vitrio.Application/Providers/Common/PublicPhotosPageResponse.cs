namespace Vitrio.Application.Providers.Common;

public sealed record PublicPhotosPageResponse(IReadOnlyList<PublicPhotoResponse> Items, bool HasMore);
