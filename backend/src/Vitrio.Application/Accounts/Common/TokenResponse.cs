namespace Vitrio.Application.Accounts.Common;

public sealed record TokenResponse(string AccessToken, int ExpiresIn, string RefreshToken);
