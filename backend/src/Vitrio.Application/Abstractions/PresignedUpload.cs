namespace Vitrio.Application.Abstractions;

public sealed record PresignedUpload(string Url, DateTimeOffset ExpiresAt);
