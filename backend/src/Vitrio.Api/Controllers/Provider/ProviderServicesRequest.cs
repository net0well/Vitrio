using Vitrio.Application.Providers.SetProviderServices;

namespace Vitrio.Api.Controllers.Provider;

public sealed record ProviderServicesRequest(IReadOnlyList<ProviderServiceRequest>? Services)
{
    public SetProviderServicesCommand ToCommand() => new((Services ?? []).Select(service => service.ToInput()).ToList());
}
