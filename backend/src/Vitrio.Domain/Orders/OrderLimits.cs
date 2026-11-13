using Vitrio.Shared.Results;

namespace Vitrio.Domain.Orders;

public static class OrderLimits
{
    public const int MaxOpenPerCustomer = 10;
    public const int MaxWaitingPerCategory = 3;

    public static Result Check(int openOrders, int waitingInCategory)
    {
        if (openOrders >= MaxOpenPerCustomer)
        {
            return OrderErrors.TooManyOpenOrders;
        }

        return waitingInCategory >= MaxWaitingPerCategory ? OrderErrors.TooManyWaitingInCategory : Result.Success();
    }
}
