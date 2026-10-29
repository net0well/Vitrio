using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RemoveDevice;

public sealed record RemoveDeviceCommand(string InstallationId) : ICommand<Result>;
