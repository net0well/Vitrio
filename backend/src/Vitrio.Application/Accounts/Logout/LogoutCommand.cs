using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.Logout;

public sealed record LogoutCommand(bool AllDevices) : ICommand<Result>;
