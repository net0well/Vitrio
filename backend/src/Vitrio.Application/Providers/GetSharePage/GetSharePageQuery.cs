using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.GetSharePage;

public sealed record GetSharePageQuery(Guid ProviderId) : IQuery<Result<SharePageResponse>>;
