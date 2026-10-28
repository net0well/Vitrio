using Vitrio.Domain.Common;

namespace Vitrio.Application.Events;

public interface IDomainEventHandler<in TEvent>
    where TEvent : DomainEvent
{
    ValueTask Handle(TEvent domainEvent, CancellationToken cancellationToken);
}
