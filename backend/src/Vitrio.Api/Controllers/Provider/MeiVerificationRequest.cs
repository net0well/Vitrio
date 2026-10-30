using Vitrio.Application.Providers.VerifyMei;

namespace Vitrio.Api.Controllers.Provider;

public sealed record MeiVerificationRequest(string? Cnpj)
{
    public VerifyMeiCommand ToCommand() => new(Cnpj ?? string.Empty);
}
