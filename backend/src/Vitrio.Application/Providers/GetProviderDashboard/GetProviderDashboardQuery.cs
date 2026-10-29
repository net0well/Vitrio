using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.GetProviderDashboard;

public sealed record GetProviderDashboardQuery : IQuery<Result<ProviderDashboardResponse>>;
