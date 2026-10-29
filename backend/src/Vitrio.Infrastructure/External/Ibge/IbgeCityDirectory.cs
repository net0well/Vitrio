using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Infrastructure.External.Ibge;

internal sealed partial class IbgeCityDirectory(HttpClient http, HybridCache cache, ILogger<IbgeCityDirectory> logger) : ICityDirectory
{
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromDays(1),
        LocalCacheExpiration = TimeSpan.FromHours(1)
    };

    public async Task<Result<IReadOnlyList<IbgeCity>>> ListAsync(string state, CancellationToken cancellationToken)
    {
        try
        {
            var cities = await cache.GetOrCreateAsync(
                $"ibge:municipios:{state}",
                (http, state),
                static async (context, token) => await FetchAsync(context.http, context.state, token),
                CacheOptions,
                cancellationToken: cancellationToken);
            return cities;
        }
        catch (Exception exception) when (IsUnavailable(exception, cancellationToken))
        {
            LogUnavailable(logger, exception.GetType().Name);
            return AccountErrors.CepUnavailable;
        }
    }

    private static async Task<List<IbgeCity>> FetchAsync(HttpClient http, string state, CancellationToken cancellationToken)
    {
        var municipalities = await http.GetFromJsonAsync<List<IbgeMunicipality>>(
            $"localidades/estados/{state}/municipios?view=nivelado",
            cancellationToken) ?? [];
        if (municipalities.Count == 0)
        {
            throw new HttpRequestException($"IBGE returned no municipalities for {state}.");
        }

        return municipalities.Select(municipality => new IbgeCity(municipality.Id, municipality.Name)).ToList();
    }

    private static bool IsUnavailable(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException or JsonException or TimeoutRejectedException or BrokenCircuitException
        || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);

    [LoggerMessage(Level = LogLevel.Warning, Message = "City directory is unavailable: {FailureType}")]
    private static partial void LogUnavailable(ILogger logger, string failureType);
}
