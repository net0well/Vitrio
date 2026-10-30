using Vitrio.Application.Providers.CreateProvider;

namespace Vitrio.Api.Controllers.Provider;

public sealed record CreateProviderRequest(string? DisplayName, string? Headline)
{
    public CreateProviderCommand ToCommand() => new(DisplayName ?? string.Empty, Headline ?? string.Empty);
}
