namespace Vitrio.Infrastructure.Events;

internal sealed record ReservedDelivery(Guid MessageId, string Handler, int Attempt);
