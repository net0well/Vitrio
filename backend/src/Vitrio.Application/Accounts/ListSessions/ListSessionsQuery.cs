using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ListSessions;

public sealed record ListSessionsQuery : IQuery<Result<IReadOnlyList<SessionResponse>>>;
