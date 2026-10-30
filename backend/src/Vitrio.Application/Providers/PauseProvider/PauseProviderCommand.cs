using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.PauseProvider;

public sealed record PauseProviderCommand : ICommand<Result<ProviderStatusResponse>>;
