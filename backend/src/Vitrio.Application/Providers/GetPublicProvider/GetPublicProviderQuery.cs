using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.GetPublicProvider;

public sealed record GetPublicProviderQuery(Guid ProviderId, double? Latitude, double? Longitude) : IQuery<Result<ProviderPublicResponse>>;
