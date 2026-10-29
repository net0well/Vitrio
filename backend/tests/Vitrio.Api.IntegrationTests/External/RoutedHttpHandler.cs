using System.Net;
using System.Text;

namespace Vitrio.Api.IntegrationTests.External;

internal sealed class RoutedHttpHandler(IReadOnlyDictionary<string, string> bodiesByPathPart) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.ToString();
        var body = bodiesByPathPart.FirstOrDefault(route => url.Contains(route.Key, StringComparison.Ordinal)).Value;
        return Task.FromResult(body is null
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
    }
}
