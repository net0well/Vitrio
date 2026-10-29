using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Domain.Common;

namespace Vitrio.Api.IntegrationTests.Controllers;

public sealed class ApiControllerBaseTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData(ErrorType.NotFound, HttpStatusCode.NotFound)]
    [InlineData(ErrorType.Conflict, HttpStatusCode.Conflict)]
    [InlineData(ErrorType.Forbidden, HttpStatusCode.Forbidden)]
    [InlineData(ErrorType.Unauthorized, HttpStatusCode.Unauthorized)]
    [InlineData(ErrorType.Unprocessable, HttpStatusCode.UnprocessableEntity)]
    [InlineData(ErrorType.RateLimited, HttpStatusCode.TooManyRequests)]
    [InlineData(ErrorType.Unavailable, HttpStatusCode.ServiceUnavailable)]
    [InlineData(ErrorType.Failure, HttpStatusCode.InternalServerError)]
    public async Task Failure_ReturnsProblemWithStatusAndErrorCode(ErrorType type, HttpStatusCode status)
    {
        var response = await _client.GetAsync($"/api/v1/probe/errors/{type}", TestContext.Current.CancellationToken);

        var problem = await response.AssertProblemAsync(status, ProbeController.FailureCode);
        Assert.Equal(ProbeController.FailureMessage, problem.GetProperty("detail").GetString());
    }

    [Fact]
    public async Task ConcurrencyConflict_ReturnsConflictProblem()
    {
        var response = await _client.GetAsync("/api/v1/probe/concurrency-conflict", TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Conflict, "conflict");
    }

    [Fact]
    public async Task ValidationFailure_ReturnsFieldErrorsInCamelCase()
    {
        var response = await _client.GetAsync("/api/v1/probe/validation", TestContext.Current.CancellationToken);

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        var errors = problem.GetProperty("errors");
        Assert.Equal("Informe o nome.", errors.GetProperty("displayName")[0].GetString());
        Assert.Equal("CEP inválido.", errors.GetProperty("address.postalCode")[0].GetString());
    }

    [Fact]
    public async Task SuccessWithValue_ReturnsOkWithBody()
    {
        var response = await _client.GetAsync("/api/v1/probe/value", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        Assert.Equal("ok", body.GetProperty("name").GetString());
    }

    [Fact]
    public async Task SuccessWithoutValue_ReturnsNoContent()
    {
        var response = await _client.GetAsync("/api/v1/probe/nothing", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
