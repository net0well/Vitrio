namespace Vitrio.Application.Abstractions;

public sealed record AccessToken(string Value, int ExpiresInSeconds);
