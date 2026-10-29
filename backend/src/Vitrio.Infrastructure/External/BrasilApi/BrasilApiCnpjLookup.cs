using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using Vitrio.Application.Abstractions;
using Vitrio.Domain.Providers;
using Vitrio.Shared.Results;

namespace Vitrio.Infrastructure.External.BrasilApi;

internal sealed partial class BrasilApiCnpjLookup(HttpClient http, ILogger<BrasilApiCnpjLookup> logger) : ICnpjLookup
{
    private const int ActiveRegistration = 2;

    public async Task<Result<CnpjLookupResult?>> FindAsync(Cnpj cnpj, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await http.GetAsync($"cnpj/v1/{cnpj.Value}", cancellationToken);
            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                return Result<CnpjLookupResult?>.Success(null);
            }

            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadFromJsonAsync<BrasilApiCnpjResponse>(cancellationToken);
            if (body is null)
            {
                return Result<CnpjLookupResult?>.Success(null);
            }

            return new CnpjLookupResult(body.CompanyName, body.RegistrationStatus == ActiveRegistration, body.OptedForMei == true);
        }
        catch (Exception exception) when (IsUnavailable(exception, cancellationToken))
        {
            LogUnavailable(logger, exception.GetType().Name);
            return ProviderErrors.CnpjUnavailable;
        }
    }

    private static bool IsUnavailable(Exception exception, CancellationToken cancellationToken) =>
        exception is HttpRequestException or JsonException or TimeoutRejectedException or BrokenCircuitException
        || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);

    [LoggerMessage(Level = LogLevel.Warning, Message = "CNPJ lookup is unavailable: {FailureType}")]
    private static partial void LogUnavailable(ILogger logger, string failureType);
}
