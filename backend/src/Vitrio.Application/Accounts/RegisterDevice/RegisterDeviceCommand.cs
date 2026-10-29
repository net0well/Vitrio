using Mediator;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.RegisterDevice;

public sealed record RegisterDeviceCommand(string InstallationId, DevicePlatform Platform, string PushToken) : ICommand<Result>
{
    public override string ToString() => nameof(RegisterDeviceCommand);
}
