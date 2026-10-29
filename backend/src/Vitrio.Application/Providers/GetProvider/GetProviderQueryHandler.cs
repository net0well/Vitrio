using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Providers.Common;
using Vitrio.Application.Providers.Repositories;
using Vitrio.Domain.Common;
using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.GetProvider;

public sealed class GetProviderQueryHandler(ICurrentUser currentUser, IProviderSearchableRepository providers, IClock clock)
    : IQueryHandler<GetProviderQuery, Result<ProviderPrivateResponse>>
{
    public async ValueTask<Result<ProviderPrivateResponse>> Handle(GetProviderQuery query, CancellationToken cancellationToken) =>
        await providers.GetPrivateAsync(currentUser.Id, clock.UtcNow, cancellationToken) is { } provider
            ? provider
            : ProviderErrors.NotFound;
}
