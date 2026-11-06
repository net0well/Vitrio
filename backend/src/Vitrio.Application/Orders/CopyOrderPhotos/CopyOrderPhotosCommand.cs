using Mediator;
using Vitrio.Application.Orders.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.CopyOrderPhotos;

public sealed record CopyOrderPhotosCommand(Guid OrderId) : ICommand<Result<IReadOnlyList<OrderPhotoCopyResponse>>>;
