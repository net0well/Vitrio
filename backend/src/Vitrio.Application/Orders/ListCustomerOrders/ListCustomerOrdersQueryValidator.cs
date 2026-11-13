using FluentValidation;

namespace Vitrio.Application.Orders.ListCustomerOrders;

internal sealed class ListCustomerOrdersQueryValidator : AbstractValidator<ListCustomerOrdersQuery>
{
    public ListCustomerOrdersQueryValidator()
    {
        RuleFor(query => query.Status).IsInEnum();
        RuleFor(query => query.Page).InclusiveBetween(1, ListCustomerOrdersQuery.MaxPage);
    }
}
