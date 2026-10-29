using Mediator;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.RegisterDevice;

public sealed record RegisterDeviceCommand(string InstallationId, DevicePlatform Platform, string PushToken) : ICommand<Result>
{
    public override string ToString() => nameof(RegisterDeviceCommand);
}
