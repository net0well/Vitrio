using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.SetProviderCategories;

public sealed record SetProviderCategoriesCommand(IReadOnlyList<int> CategoryIds, int PrimaryCategoryId) : ICommand<Result>;
