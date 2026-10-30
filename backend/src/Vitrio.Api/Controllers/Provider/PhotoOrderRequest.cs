using Vitrio.Application.Providers.ReorderPhotos;

namespace Vitrio.Api.Controllers.Provider;

public sealed record PhotoOrderRequest(IReadOnlyList<Guid>? Ids)
{
    public ReorderPhotosCommand ToCommand() => new(Ids ?? []);
}
