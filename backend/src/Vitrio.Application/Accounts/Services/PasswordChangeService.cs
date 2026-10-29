using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Emails;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Services;

internal sealed class PasswordChangeService(
    IPasswordService passwords,
    ISessionWritableRepository sessions,
    ISessionService sessionService,
    IEmailSender emailSender,
    IClock clock) : IPasswordChangeService
{
    public async Task<IReadOnlyCollection<Guid>> ChangeAsync(
        User user,
        string newPassword,
        string revocationReason,
        Guid? sessionToKeep,
        CancellationToken cancellationToken)
    {
        user.ChangePassword(passwords.Hash(newPassword), clock);
        var others = (await sessions.ListUnrevokedForUserAsync(user.Id, cancellationToken))
            .Where(session => session.Id != sessionToKeep)
            .ToList();
        return await sessionService.RevokeAsync(others, revocationReason, cancellationToken);
    }

    public async Task NotifyAsync(User user, CancellationToken cancellationToken)
    {
        var firstName = DisplayName.FromStored(user.DisplayName).FirstName;
        await emailSender.SendAsync(new PasswordChangedEmail(user.Email, firstName, user.PasswordChangedAt), cancellationToken);
    }
}
