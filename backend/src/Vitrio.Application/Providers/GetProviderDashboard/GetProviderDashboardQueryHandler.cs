using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.GetProviderDashboard;

public sealed class GetProviderDashboardQueryHandler(ICurrentUser currentUser, IProviderSearchableRepository providers, IClock clock)
    : IQueryHandler<GetProviderDashboardQuery, Result<ProviderDashboardResponse>>
{
    public async ValueTask<Result<ProviderDashboardResponse>> Handle(GetProviderDashboardQuery query, CancellationToken cancellationToken) =>
        await providers.GetDashboardAsync(currentUser.Id, clock.UtcNow, cancellationToken) is { } dashboard
            ? dashboard
            : ProviderErrors.NotFound;
}
