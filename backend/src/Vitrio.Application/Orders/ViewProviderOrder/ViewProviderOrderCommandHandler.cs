using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Services;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.ViewProviderOrder;

public sealed class ViewProviderOrderCommandHandler(IProviderOrderViewService views, IUnitOfWork unitOfWork)
    : ICommandHandler<ViewProviderOrderCommand, Result<OrderProviderView>>
{
    public async ValueTask<Result<OrderProviderView>> Handle(ViewProviderOrderCommand command, CancellationToken cancellationToken)
    {
        var view = await views.GetAsync(command.OrderId, cancellationToken);
        if (view.IsSuccess)
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return view;
    }
}
