using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ProcessOrderPhoto;

public sealed record ProcessOrderPhotoCommand(Guid OrderId, Guid PhotoId) : ICommand<Result>;
