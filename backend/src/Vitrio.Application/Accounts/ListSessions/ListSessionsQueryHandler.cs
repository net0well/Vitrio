using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Accounts.ListSessions;

public sealed class ListSessionsQueryHandler(ICurrentUser currentUser, ISessionSearchableRepository sessions, IClock clock)
    : IQueryHandler<ListSessionsQuery, Result<IReadOnlyList<SessionResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<SessionResponse>>> Handle(ListSessionsQuery query, CancellationToken cancellationToken)
    {
        var list = await sessions.ListActiveForUserAsync(currentUser.Id, currentUser.SessionId, clock.UtcNow, cancellationToken);
        return Result<IReadOnlyList<SessionResponse>>.Success(list);
    }
}
