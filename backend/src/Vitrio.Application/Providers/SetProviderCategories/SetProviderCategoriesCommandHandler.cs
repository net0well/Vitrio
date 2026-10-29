using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.SetProviderCategories;

public sealed class SetProviderCategoriesCommandHandler(
    ICurrentUser currentUser,
    IProviderWritableRepository providers,
    ICategorySearchableRepository categories,
    IUnitOfWork unitOfWork,
    IClock clock) : ICommandHandler<SetProviderCategoriesCommand, Result>
{
    public async ValueTask<Result> Handle(SetProviderCategoriesCommand command, CancellationToken cancellationToken)
    {
        var provider = await providers.GetByUserIdAsync(currentUser.Id, cancellationToken);
        if (provider is null)
        {
            return ProviderErrors.NotFound;
        }

        if (await categories.CountActiveAsync(command.CategoryIds, cancellationToken) != command.CategoryIds.Count)
        {
            return ProviderErrors.UnknownCategory;
        }

        var result = provider.SetCategories(command.CategoryIds, command.PrimaryCategoryId, clock);
        if (result.IsFailure)
        {
            return result;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
