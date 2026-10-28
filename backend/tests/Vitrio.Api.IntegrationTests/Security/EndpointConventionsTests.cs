using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Infrastructure;

namespace Vitrio.Api.IntegrationTests.Security;

public sealed class EndpointConventionsTests(ApiFactory factory)
{
    [Fact]
    public void EveryControllerAction_DeclaresAuthorizationExplicitly()
    {
        var missing = ControllerEndpoints()
            .Where(endpoint => endpoint.Metadata.GetMetadata<IAuthorizeData>() is null
                && endpoint.Metadata.GetMetadata<IAllowAnonymous>() is null)
            .Select(endpoint => endpoint.DisplayName)
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void EveryReadOnlyControllerAction_DeclaresRateLimitPolicy()
    {
        var missing = ControllerEndpoints()
            .Where(IsReadOnly)
            .Where(endpoint => endpoint.Metadata.GetMetadata<EnableRateLimitingAttribute>() is null
                && endpoint.Metadata.GetMetadata<DisableRateLimitingAttribute>() is null)
            .Select(endpoint => endpoint.DisplayName)
            .ToList();

        Assert.Empty(missing);
    }

    private List<Endpoint> ControllerEndpoints()
    {
        var endpoints = factory.Services
            .GetRequiredService<EndpointDataSource>()
            .Endpoints
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
            .ToList();

        Assert.NotEmpty(endpoints);
        return endpoints;
    }

    private static bool IsReadOnly(Endpoint endpoint) =>
        endpoint.Metadata.GetMetadata<HttpMethodMetadata>()?.HttpMethods.All(method =>
            HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method)) ?? true;
}
