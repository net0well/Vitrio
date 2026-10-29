namespace Vitrio.Application.Accounts.Emails;

public sealed record PasswordResetCodeEmail(string To, string FirstName, string Code) : AccountEmail(To, FirstName);
