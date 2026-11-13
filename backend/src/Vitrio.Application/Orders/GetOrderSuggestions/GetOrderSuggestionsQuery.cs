using Mediator;
using Vitrio.Application.Search.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.GetOrderSuggestions;

public sealed record GetOrderSuggestionsQuery(Guid OrderId) : IQuery<Result<IReadOnlyList<ProviderSearchItem>>>;
