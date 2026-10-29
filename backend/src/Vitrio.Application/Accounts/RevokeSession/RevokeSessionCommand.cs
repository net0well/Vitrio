using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand<Result>;
