using FluentValidation;

namespace Vitrio.Application.Orders.ListProviderOrders;

internal sealed class ListProviderOrdersQueryValidator : AbstractValidator<ListProviderOrdersQuery>
{
    public ListProviderOrdersQueryValidator()
    {
        RuleFor(query => query.Tab).IsInEnum();
        RuleFor(query => query.Page).InclusiveBetween(1, ListProviderOrdersQuery.MaxPage);
    }
}
