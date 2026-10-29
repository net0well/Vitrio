using System.Text.Json.Serialization;
using Vitrio.Application.Accounts.RegisterDevice;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.Controllers.Me;

public sealed record RegisterDeviceRequest([property: JsonRequired] DevicePlatform Platform, string? PushToken)
{
    public RegisterDeviceCommand ToCommand(string installationId) => new(installationId, Platform, PushToken ?? string.Empty);

    public override string ToString() => nameof(RegisterDeviceRequest);
}
