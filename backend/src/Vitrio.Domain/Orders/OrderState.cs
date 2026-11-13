namespace Vitrio.Domain.Orders;

public sealed record OrderState(
    OrderStatus Status,
    DateTimeOffset RespondBy,
    DateTimeOffset? QuoteValidUntil,
    DateTimeOffset? ScheduledStart,
    int RescheduleCount)
{
    public IReadOnlyList<OrderAction> AllowedActionsFor(OrderParty party, DateTimeOffset now) => party switch
    {
        OrderParty.Customer => CustomerActions(now),
        OrderParty.Provider => ProviderActions(now),
        _ => []
    };

    private List<OrderAction> CustomerActions(DateTimeOffset now) => Status switch
    {
        OrderStatus.Sent => [OrderAction.Cancel],
        OrderStatus.Quoted when now < QuoteValidUntil => [OrderAction.AcceptQuote, OrderAction.DeclineQuote, OrderAction.Cancel],
        OrderStatus.Quoted => [OrderAction.Cancel],
        OrderStatus.Scheduled when now >= ScheduledStart => [OrderAction.Cancel, OrderAction.Complete],
        OrderStatus.Scheduled => [OrderAction.Cancel],
        _ => []
    };

    private List<OrderAction> ProviderActions(DateTimeOffset now) => Status switch
    {
        OrderStatus.Sent when now < RespondBy => [OrderAction.SendQuote, OrderAction.Decline],
        OrderStatus.Quoted => [OrderAction.ReplaceQuote],
        OrderStatus.Scheduled when now >= ScheduledStart => [OrderAction.Cancel, OrderAction.Complete],
        OrderStatus.Scheduled when RescheduleCount < Order.MaxReschedules => [OrderAction.Reschedule, OrderAction.Cancel],
        OrderStatus.Scheduled => [OrderAction.Cancel],
        _ => []
    };
}
