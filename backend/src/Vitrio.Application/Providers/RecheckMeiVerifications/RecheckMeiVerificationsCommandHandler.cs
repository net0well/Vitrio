using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Application.Providers.Services;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.RecheckMeiVerifications;

public sealed class RecheckMeiVerificationsCommandHandler(
    IProviderSearchableRepository providerQueries,
    IProviderWritableRepository providers,
    IMeiCheckService meiCheck,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecheckMeiVerificationsCommand, Result>
{
    private static readonly TimeSpan PendingRetryInterval = TimeSpan.FromHours(6);
    private static readonly TimeSpan PendingGiveUpAfter = TimeSpan.FromDays(3);
    private static readonly TimeSpan RevalidationInterval = TimeSpan.FromDays(30);

    public async ValueTask<Result> Handle(RecheckMeiVerificationsCommand command, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var due = await providerQueries.ListMeiDueAsync(now - PendingRetryInterval, now - RevalidationInterval, command.BatchSize, cancellationToken);
        foreach (var providerId in due)
        {
            var provider = await providers.GetByIdAsync(providerId, cancellationToken);
            if (provider?.Mei is not { } mei)
            {
                continue;
            }

            if (mei is { Status: MeiStatus.Pending, PendingSince: { } since } && since <= now - PendingGiveUpAfter)
            {
                provider.GiveUpPendingMei(clock);
            }
            else
            {
                var check = await meiCheck.CheckAsync(Cnpj.FromStored(mei.Cnpj), cancellationToken);
                provider.RecordMeiCheck(Cnpj.FromStored(mei.Cnpj), check.Outcome, check.CompanyName, clock);
                await Task.Delay(command.PauseBetweenLookups, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
