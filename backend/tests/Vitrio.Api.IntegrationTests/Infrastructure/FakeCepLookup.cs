using System.Collections.Concurrent;
using System.Globalization;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class FakeCepLookup : ICepLookup
{
    public const string Jaboticabal = "14870000";
    public const string Unknown = "99999999";
    public const string Unreachable = "88888888";
    public const string JaboticabalWithoutPoint = "14871000";

    private static int _lastRegistered = 60_000_000;

    private readonly ConcurrentDictionary<string, CepLookupResult> _registered = new(StringComparer.Ordinal);

    public string Register(int cityIbgeCode, string cityName, string state, double latitude, double longitude)
    {
        var cep = Interlocked.Increment(ref _lastRegistered).ToString("D8", CultureInfo.InvariantCulture);
        _registered[cep] = new CepLookupResult("Rua de Teste", "Bairro de Teste", cityName, state, cityIbgeCode, latitude, longitude);
        return cep;
    }

    public Task<Result<CepLookupResult?>> FindAsync(Cep cep, CancellationToken cancellationToken) => Task.FromResult(cep.Value switch
    {
        Jaboticabal => Result<CepLookupResult?>.Success(
            new CepLookupResult("Rua Rui Barbosa", "Centro", "Jaboticabal", "SP", null, -21.2547, -48.3221)),
        JaboticabalWithoutPoint => Result<CepLookupResult?>.Success(
            new CepLookupResult(null, null, "Jaboticabal", "SP", 3524303, null, null)),
        Unreachable => AccountErrors.CepUnavailable,
        var other when _registered.TryGetValue(other, out var registered) => Result<CepLookupResult?>.Success(registered),
        _ => Result<CepLookupResult?>.Success(null)
    });
}
