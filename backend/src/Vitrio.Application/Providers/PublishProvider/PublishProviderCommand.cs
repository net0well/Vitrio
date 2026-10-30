using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.PublishProvider;

public sealed record PublishProviderCommand : ICommand<Result<ProviderStatusResponse>>;
