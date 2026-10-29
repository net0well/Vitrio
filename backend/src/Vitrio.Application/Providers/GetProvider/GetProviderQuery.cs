using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.GetProvider;

public sealed record GetProviderQuery : IQuery<Result<ProviderPrivateResponse>>;
