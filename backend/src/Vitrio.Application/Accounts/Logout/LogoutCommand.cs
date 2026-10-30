using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.Logout;

public sealed record LogoutCommand(bool AllDevices) : ICommand<Result>;
