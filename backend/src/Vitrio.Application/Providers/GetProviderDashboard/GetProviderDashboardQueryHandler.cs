using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Orders.Common;
using Vitrio.Application.Orders.Repositories;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;
using Vitrio.Shared.Time;

namespace Vitrio.Application.Providers.GetProviderDashboard;

public sealed class GetProviderDashboardQueryHandler(
    ICurrentUser currentUser,
    IProviderSearchableRepository providers,
    IOrderSearchableRepository orders,
    IClock clock) : IQueryHandler<GetProviderDashboardQuery, Result<ProviderDashboardResponse>>
{
    public async ValueTask<Result<ProviderDashboardResponse>> Handle(GetProviderDashboardQuery query, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var profile = await providers.GetDashboardAsync(currentUser.Id, now, cancellationToken);
        if (profile is null)
        {
            return ProviderErrors.NotFound;
        }

        var numbers = ProviderOrderNumbers.None;
        if (profile.TimeZone is { } timeZone)
        {
            var (monthStart, nextMonthStart) = CityTime.MonthOf(now, CityTime.Zone(timeZone));
            numbers = await orders.GetProviderNumbersAsync(profile.Id, monthStart, nextMonthStart, now, cancellationToken);
        }

        return ProviderDashboardResponse.From(profile, numbers);
    }
}
