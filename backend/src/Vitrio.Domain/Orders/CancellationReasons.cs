namespace Vitrio.Domain.Orders;

public static class CancellationReasons
{
    private static readonly HashSet<CancellationReason> Customer =
    [
        CancellationReason.ResolvedAnotherWay,
        CancellationReason.TimeNoLongerWorks,
        CancellationReason.NewTimeDoesNotWork,
        CancellationReason.FoundAnotherProfessional,
        CancellationReason.Other
    ];

    private static readonly HashSet<CancellationReason> Provider =
    [
        CancellationReason.Unforeseen,
        CancellationReason.CannotMakeIt,
        CancellationReason.CustomerAskedToCancel,
        CancellationReason.Other
    ];

    private static readonly HashSet<CancellationReason> System =
    [
        CancellationReason.AccountDeleted,
        CancellationReason.AccountSuspended
    ];

    public static bool Allows(OrderParty party, CancellationReason reason) => party switch
    {
        OrderParty.Customer => Customer.Contains(reason),
        OrderParty.Provider => Provider.Contains(reason),
        OrderParty.System => System.Contains(reason),
        _ => false
    };
}
