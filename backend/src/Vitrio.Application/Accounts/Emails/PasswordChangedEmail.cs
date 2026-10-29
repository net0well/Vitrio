namespace Vitrio.Application.Accounts.Emails;

public sealed record PasswordChangedEmail(string To, string FirstName, DateTimeOffset ChangedAt) : AccountEmail(To, FirstName);
