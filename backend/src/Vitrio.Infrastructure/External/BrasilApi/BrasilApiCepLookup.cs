using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Infrastructure.External.BrasilApi;

internal sealed partial class BrasilApiCepLookup(HttpClient http, ILogger<BrasilApiCepLookup> logger) : ICepLookup
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<Result<CepLookupResult?>> FindAsync(Cep cep, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync($"cep/v2/{cep.Value}", cancellationToken);
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                return Result<CepLookupResult?>.Success(null);
            }

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<BrasilApiCepResponse>(Json, cancellationToken);
            if (body is null || string.IsNullOrWhiteSpace(body.City) || string.IsNullOrWhiteSpace(body.State))
            {
                return Result<CepLookupResult?>.Success(null);
            }

            return new CepLookupResult(
                body.Street,
                body.Neighborhood,
                body.City,
                body.State.ToUpperInvariant(),
                int.TryParse(body.Ibge?.City, NumberStyles.None, CultureInfo.InvariantCulture, out var cityCode) ? cityCode : null,
                Parse(body.Location?.Coordinates?.Latitude),
                Parse(body.Location?.Coordinates?.Longitude));
        }
        catch (Exception exception) when (IsUnavailable(exception, cancellationToken))
        {
            LogUnavailable(logger, exception.GetType().Name);
            return AccountErrors.CepUnavailable;
        }
    }

    private static double? Parse(string? value) =>
        double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;

    private static bool IsUnavailable(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException or JsonException or TimeoutRejectedException or BrokenCircuitException
        || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CEP lookup is unavailable: {FailureType}")]
    private static partial void LogUnavailable(ILogger logger, string failureType);
}
