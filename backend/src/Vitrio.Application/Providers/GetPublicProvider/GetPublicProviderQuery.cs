using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.GetPublicProvider;

public sealed record GetPublicProviderQuery(Guid ProviderId, double? Latitude, double? Longitude) : IQuery<Result<ProviderPublicResponse>>;
