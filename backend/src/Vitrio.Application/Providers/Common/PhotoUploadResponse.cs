namespace Vitrio.Application.Providers.Common;

public sealed record PhotoUploadResponse(string UploadUrl, string ObjectKey, DateTimeOffset ExpiresAt);
