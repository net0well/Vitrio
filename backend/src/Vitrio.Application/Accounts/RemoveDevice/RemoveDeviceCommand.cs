using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.RemoveDevice;

public sealed record RemoveDeviceCommand(string InstallationId) : ICommand<Result>;
