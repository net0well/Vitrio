namespace Vitrio.Domain.Orders;

public enum CancellationReason
{
    ResolvedAnotherWay,
    TimeNoLongerWorks,
    NewTimeDoesNotWork,
    FoundAnotherProfessional,
    Unforeseen,
    CannotMakeIt,
    CustomerAskedToCancel,
    Other,
    AccountDeleted,
    AccountSuspended
}
