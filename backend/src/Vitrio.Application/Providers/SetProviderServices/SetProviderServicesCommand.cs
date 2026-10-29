using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.SetProviderServices;

public sealed record SetProviderServicesCommand(IReadOnlyList<ServiceInput> Services) : ICommand<Result>;
