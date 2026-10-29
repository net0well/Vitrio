using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ListSessions;

public sealed record ListSessionsQuery : IQuery<Result<IReadOnlyList<SessionResponse>>>;
