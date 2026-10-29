using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.SetProviderCategories;

public sealed record SetProviderCategoriesCommand(IReadOnlyList<int> CategoryIds, int PrimaryCategoryId) : ICommand<Result>;
