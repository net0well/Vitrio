using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Vitrio.Api.IntegrationTests.Infrastructure;

internal sealed class TestClientAddressStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use((context, nextMiddleware) =>
        {
            if (IPAddress.TryParse(context.Request.Headers[TestClientAddress.Header], out var address))
            {
                context.Connection.RemoteIpAddress = address;
            }

            return nextMiddleware(context);
        });
        next(app);
    };
}
