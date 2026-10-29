using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.GetProvider;

public sealed record GetProviderQuery : IQuery<Result<ProviderPrivateResponse>>;
