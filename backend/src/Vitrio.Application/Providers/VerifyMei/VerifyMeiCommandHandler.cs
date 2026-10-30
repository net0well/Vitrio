using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Application.Providers.Services;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.VerifyMei;

public sealed class VerifyMeiCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    IProviderSearchableRepository providerQueries,
    IMeiCheckService meiCheck,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<VerifyMeiCommand, Result<MeiVerificationResponse>>
{
    public async ValueTask<Result<MeiVerificationResponse>> Handle(VerifyMeiCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        var cnpj = Cnpj.Create(command.Cnpj).Value;
        if (await providerQueries.IsCnpjInUseAsync(cnpj.Value, provider.Id, cancellationToken))
        {
            return ProviderErrors.CnpjInUse;
        }

        var check = await meiCheck.CheckAsync(cnpj, cancellationToken);
        provider.RecordMeiCheck(cnpj, check.Outcome, check.CompanyName, clock);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        var mei = provider.Mei!;
        return new MeiVerificationResponse(mei.Status, mei.CompanyName, MeiReasons.MessageFor(mei.ReasonCode));
    }
}
