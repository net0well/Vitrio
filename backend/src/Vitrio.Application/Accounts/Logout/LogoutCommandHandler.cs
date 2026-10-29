using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Logout;

public sealed class LogoutCommandHandler(
    ICurrentUser currentUser,
    ISessionWritableRepository sessions,
    ISessionService sessionService,
    ISessionStatusCache sessionStatusCache,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork) : ICommandHandler<LogoutCommand, Result>
{
    public async ValueTask<Result> Handle(LogoutCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Session> targets = command.AllDevices
            ? await sessions.ListUnrevokedForUserAsync(currentUser.Id, cancellationToken)
            : await sessions.GetForUserAsync(currentUser.SessionId, currentUser.Id, cancellationToken) is { } current ? [current] : [];

        var reason = command.AllDevices ? SessionRevokedReasons.SignedOutEverywhere : SessionRevokedReasons.SignedOut;
        var revoked = await sessionService.RevokeAsync(targets, reason, cancellationToken);
        auditLog.Record(
            command.AllDevices ? AuditActions.SignedOutEverywhere : AuditActions.SignedOut,
            currentUser.Id,
            AuditTargets.Session,
            currentUser.SessionId);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await sessionStatusCache.ForgetAsync(revoked, cancellationToken);
        return Result.Success();
    }
}
