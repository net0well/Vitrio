using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.GetMe;

public sealed record GetMeQuery : IQuery<Result<MeResponse>>;
