using Vitrio.Application.Abstractions;
using Vitrio.Application.Events;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Domain.Orders;

namespace Vitrio.Application.Orders.Events;

public sealed class ProcessOrderPhotos(IOrderSearchableRepository orders, IPhotoProcessingQueue queue) : IDomainEventHandler<OrderCreated>
{
    public async ValueTask Handle(OrderCreated domainEvent, CancellationToken cancellationToken)
    {
        foreach (var photoId in await orders.ListProcessingPhotoIdsAsync(domainEvent.OrderId, cancellationToken))
        {
            queue.EnqueueOrderPhoto(domainEvent.OrderId, photoId);
        }
    }
}
