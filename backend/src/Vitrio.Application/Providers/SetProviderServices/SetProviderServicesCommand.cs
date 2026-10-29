using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.SetProviderServices;

public sealed record SetProviderServicesCommand(IReadOnlyList<ServiceInput> Services) : ICommand<Result>;
