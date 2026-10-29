namespace Vitrio.Application.Accounts.Emails;

public sealed record RegistrationAttemptEmail(string To, string FirstName) : AccountEmail(To, FirstName);
