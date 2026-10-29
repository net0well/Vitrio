using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class FakeCepLookup : ICepLookup
{
    public const string Jaboticabal = "14870000";
    public const string Unknown = "99999999";
    public const string Unreachable = "88888888";
    public const string JaboticabalWithoutPoint = "14871000";

    public Task<Result<CepLookupResult?>> FindAsync(Cep cep, CancellationToken cancellationToken) => Task.FromResult(cep.Value switch
    {
        Jaboticabal => Result<CepLookupResult?>.Success(
            new CepLookupResult("Rua Rui Barbosa", "Centro", "Jaboticabal", "SP", null, -21.2547, -48.3221)),
        JaboticabalWithoutPoint => Result<CepLookupResult?>.Success(
            new CepLookupResult(null, null, "Jaboticabal", "SP", 3524303, null, null)),
        Unreachable => AccountErrors.CepUnavailable,
        _ => Result<CepLookupResult?>.Success(null)
    });
}
