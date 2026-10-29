namespace Vitrio.Application.Accounts.Common;

public sealed record AuthResponse(string AccessToken, int ExpiresIn, string RefreshToken, MeResponse User);
