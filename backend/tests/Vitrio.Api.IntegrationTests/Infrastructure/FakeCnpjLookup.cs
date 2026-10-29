using System.Collections.Concurrent;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class FakeCnpjLookup : ICnpjLookup
{
    private static int _sequence = 10_000;

    private readonly ConcurrentDictionary<string, Result<CnpjLookupResult?>> _answers = new();

    public static string NewCnpj()
    {
        var digits = $"{Interlocked.Increment(ref _sequence):D8}0001";
        digits += CheckDigit(digits, [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        digits += CheckDigit(digits, [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        return digits;
    }

    public void AnswerActiveMei(string cnpj) => _answers[cnpj] = Result<CnpjLookupResult?>.Success(new CnpjLookupResult("CARLOS ELETRICISTA", true, true));

    public void AnswerNotMei(string cnpj) => _answers[cnpj] = Result<CnpjLookupResult?>.Success(new CnpjLookupResult("CARLOS SERVICOS LTDA", true, false));

    public void AnswerUnavailable(string cnpj) => _answers[cnpj] = ProviderErrors.CnpjUnavailable;

    public Task<Result<CnpjLookupResult?>> FindAsync(Cnpj cnpj, CancellationToken cancellationToken) =>
        Task.FromResult(_answers.TryGetValue(cnpj.Value, out var answer) ? answer : Result<CnpjLookupResult?>.Success(null));

    private static int CheckDigit(string digits, int[] weights)
    {
        var remainder = digits.Select((digit, index) => (digit - '0') * weights[index]).Sum() % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }
}
