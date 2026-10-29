using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RevokeSession;

public sealed class RevokeSessionCommandHandler(
    ICurrentUser currentUser,
    ISessionWritableRepository sessions,
    SessionService sessionService,
    ISessionStatusCache sessionStatusCache,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork) : ICommandHandler<RevokeSessionCommand, Result>
{
    public async ValueTask<Result> Handle(RevokeSessionCommand command, CancellationToken cancellationToken)
    {
        var session = await sessions.GetForUserAsync(command.SessionId, currentUser.Id, cancellationToken);
        if (session is null || session.RevokedAt is not null)
        {
            return AccountErrors.SessionNotFound;
        }

        var revoked = await sessionService.RevokeAsync([session], SessionRevokedReasons.RevokedByUser, cancellationToken);
        auditLog.Record(AuditActions.SessionRevoked, currentUser.Id, AuditTargets.Session, session.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await sessionStatusCache.ForgetAsync(revoked, cancellationToken);
        return Result.Success();
    }
}
