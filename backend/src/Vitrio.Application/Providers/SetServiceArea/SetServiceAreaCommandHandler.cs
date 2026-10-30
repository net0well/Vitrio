using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Application.Providers.Services;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.SetServiceArea;

public sealed class SetServiceAreaCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IServiceAreaService serviceArea,
    IAuditLog auditLog,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetServiceAreaCommand, Result<ServiceAreaSavedResponse>>
{
    public async ValueTask<Result<ServiceAreaSavedResponse>> Handle(SetServiceAreaCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var area = await serviceArea.ResolveAsync(Cep.Create(command.Cep).Value, command.Number, command.Complement, cancellationToken);
        if (area.IsFailure)
        {
            return area.Error;
        }

        var result = provider.SetServiceArea(area.Value.Address, area.Value.Point, area.Value.Approximate, command.RadiusKm, command.Neighborhoods, clock);
        if (result.IsFailure)
        {
            return result.Error;
        }

        auditLog.Record(ProviderAudit.BaseAddressChanged, currentUser.Id, ProviderAudit.Target, provider.Id);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new ServiceAreaSavedResponse(area.Value.City, area.Value.Address.Neighborhood, area.Value.Approximate);
    }
}
