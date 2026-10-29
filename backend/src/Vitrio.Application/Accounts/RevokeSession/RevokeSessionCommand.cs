using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionId) : ICommand<Result>;
