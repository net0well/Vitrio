using Vitrio.Application.Providers.SetAvatar;

namespace Vitrio.Api.Controllers.Provider;

public sealed record AvatarRequest(string? ObjectKey)
{
    public SetAvatarCommand ToCommand() => new(ObjectKey ?? string.Empty);
}
