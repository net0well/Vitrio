using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.ReorderPhotos;

public sealed record ReorderPhotosCommand(IReadOnlyList<Guid> PhotoIds) : ICommand<Result>;
