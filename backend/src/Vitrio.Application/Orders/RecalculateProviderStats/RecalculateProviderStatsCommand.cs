using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.RecalculateProviderStats;

public sealed record RecalculateProviderStatsCommand : ICommand<Result>;
