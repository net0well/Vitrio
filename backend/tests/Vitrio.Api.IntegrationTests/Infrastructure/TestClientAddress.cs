using System.Net;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public static class TestClientAddress
{
    public const string Header = "X-Test-Client-Address";

    private static int _next;

    public static HttpClient WithOwnAddress(this HttpClient client)
    {
        var number = Interlocked.Increment(ref _next);
        client.DefaultRequestHeaders.Add(Header, new IPAddress([10, (byte)(number >> 16), (byte)(number >> 8), (byte)number]).ToString());
        return client;
    }
}
