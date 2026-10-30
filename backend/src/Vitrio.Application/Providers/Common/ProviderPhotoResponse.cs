using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderPhotoResponse(Guid Id, string? Url, string? ThumbnailUrl, string? Caption, PhotoStatus Status, string? RejectionReason);
