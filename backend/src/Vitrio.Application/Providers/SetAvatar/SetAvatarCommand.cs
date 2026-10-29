using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.SetAvatar;

public sealed record SetAvatarCommand(string ObjectKey) : ICommand<Result>;
