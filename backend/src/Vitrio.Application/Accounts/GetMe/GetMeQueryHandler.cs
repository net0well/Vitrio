using Mediator;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.Repositories;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.GetMe;

public sealed class GetMeQueryHandler(ICurrentUser currentUser, IUserSearchableRepository users)
    : IQueryHandler<GetMeQuery, Result<MeResponse>>
{
    public async ValueTask<Result<MeResponse>> Handle(GetMeQuery query, CancellationToken cancellationToken) =>
        await users.GetMeAsync(currentUser.Id, cancellationToken) is { } me ? me : AccountErrors.AccountNotFound;
}
