using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ProcessAvatar;

public sealed record ProcessAvatarCommand(Guid ProviderId, string UploadKey) : ICommand<Result>;
