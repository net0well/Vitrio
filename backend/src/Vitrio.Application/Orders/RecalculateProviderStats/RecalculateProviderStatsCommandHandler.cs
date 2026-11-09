using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Orders.Services;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Orders.RecalculateProviderStats;

public sealed class RecalculateProviderStatsCommandHandler(
    IOrderSearchableRepository orders,
    IProviderWritableRepository providers,
    IProviderStatsService stats,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<RecalculateProviderStatsCommand, Result>
{
    private static readonly TimeSpan MissedRunsCovered = TimeSpan.FromDays(2);

    public async ValueTask<Result> Handle(RecalculateProviderStatsCommand command, CancellationToken cancellationToken)
    {
        var windowStart = clock.UtcNow - ProviderStats.Window;
        foreach (var providerId in await orders.ListProvidersWithOrdersBetweenAsync(windowStart - MissedRunsCovered, windowStart, cancellationToken))
        {
            var provider = await providers.GetByIdAsync(providerId, cancellationToken);
            if (provider is null)
            {
                continue;
            }

            await stats.RefreshResponseTimeAsync(provider, cancellationToken);
            await stats.RefreshLateCancellationsAsync(provider, cancellationToken);
            await unitOfWork.TrySaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
