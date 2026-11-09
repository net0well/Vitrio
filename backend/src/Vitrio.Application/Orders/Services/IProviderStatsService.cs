using Vitrio.Domain.Providers;

namespace Vitrio.Application.Orders.Services;

public interface IProviderStatsService
{
    Task RefreshResponseTimeAsync(Provider provider, CancellationToken cancellationToken);

    Task RefreshServicesDoneAsync(Provider provider, CancellationToken cancellationToken);

    Task RefreshLateCancellationsAsync(Provider provider, CancellationToken cancellationToken);
}
