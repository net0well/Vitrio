using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Application.Accounts.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Exceptions;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.RefreshSession;

public sealed class RefreshSessionCommandHandler(
    IRefreshTokenWritableRepository refreshTokens,
    ISessionWritableRepository sessions,
    IUserWritableRepository users,
    IRefreshTokenFactory refreshTokenFactory,
    ISessionService sessionService,
    ISessionStatusCache sessionStatusCache,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RefreshSessionCommand, Result<TokenResponse>>
{
    public async ValueTask<Result<TokenResponse>> Handle(RefreshSessionCommand command, CancellationToken cancellationToken)
    {
        var token = await refreshTokens.GetByHashAsync(refreshTokenFactory.Hash(command.RefreshToken), cancellationToken);
        var session = token is null ? null : await sessions.GetAsync(token.SessionId, cancellationToken);
        if (token is null || session is null)
        {
            return AccountErrors.InvalidRefreshToken;
        }

        if (token.UsedAt is not null)
        {
            await RevokeReusedFamilyAsync(session, cancellationToken);
            return AccountErrors.InvalidRefreshToken;
        }

        var user = await users.GetByIdAsync(session.UserId, cancellationToken);
        if (!token.IsUsable(clock) || !session.IsActive(clock) || user is not { Status: UserStatus.Active })
        {
            return AccountErrors.InvalidRefreshToken;
        }

        var response = await sessionService.RotateAsync(user, session, token, cancellationToken);
        return await TrySaveAsync(cancellationToken) ? response : AccountErrors.InvalidRefreshToken;
    }

    private async Task RevokeReusedFamilyAsync(Session session, CancellationToken cancellationToken)
    {
        if (session.RevokedAt is not null)
        {
            return;
        }

        var revoked = await sessionService.RevokeAsync([session], SessionRevokedReasons.RefreshReused, cancellationToken);
        auditLog.Record(AuditActions.RefreshReused, session.UserId, AuditTargets.Session, session.Id);
        if (await TrySaveAsync(cancellationToken))
        {
            await sessionStatusCache.ForgetAsync(revoked, cancellationToken);
        }
    }

    private async Task<bool> TrySaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (ConcurrencyConflictException)
        {
            return false;
        }
    }
}
