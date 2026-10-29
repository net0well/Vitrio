using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class ProfileTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task GetMe_ReturnsTheAccountData()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", auth.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await response.Content.ReadFromJsonAsync<MeResponse>(TestContext.Current.CancellationToken);
        Assert.Equal(auth.User.Id, me!.Id);
        Assert.Equal("Bruno Costa", me.DisplayName);
        Assert.Equal("BC", me.Initials);
        Assert.Null(me.PhoneNumber);
        Assert.Equal(new[] { "customer" }, me.Roles);
    }

    [Fact]
    public async Task UpdateMe_WithMobileNumber_StoresItInE164()
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { phoneNumber = "(16) 99123-4567" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var me = await (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", auth.AccessToken))
            .Content.ReadFromJsonAsync<MeResponse>(TestContext.Current.CancellationToken);
        Assert.Equal("+5516991234567", me!.PhoneNumber);
        Assert.Equal("Bruno Costa", me.DisplayName);
    }

    [Theory]
    [InlineData("(16) 3202-1234")]
    [InlineData("99123-4567")]
    public async Task UpdateMe_WithLandlineOrMissingAreaCode_ReturnsValidationErrorOnPhoneNumber(string phoneNumber)
    {
        var auth = await factory.SignInNewAccountAsync(_client);

        var response = await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { phoneNumber });

        var problem = await response.AssertProblemAsync(HttpStatusCode.BadRequest, "validation");
        Assert.True(problem.GetProperty("errors").TryGetProperty("phoneNumber", out _));
    }

    [Fact]
    public async Task UpdateMe_RenamesAndClearsThePhoneNumber()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { phoneNumber = "16991234567" });

        var response = await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { displayName = "Bruno Souza", phoneNumber = "" });

        var me = await response.Content.ReadFromJsonAsync<MeResponse>(TestContext.Current.CancellationToken);
        Assert.Equal("Bruno Souza", me!.DisplayName);
        Assert.Null(me.PhoneNumber);
    }

    [Fact]
    public async Task PhoneNumber_IsEncryptedInTheDatabase()
    {
        var auth = await factory.SignInNewAccountAsync(_client);
        await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { phoneNumber = "16991234567" });

        var stored = await factory.QueryAsync(db => db.Database
            .SqlQuery<string>($"SELECT phone_number AS \"Value\" FROM users WHERE id = {auth.User.Id}")
            .SingleAsync(TestContext.Current.CancellationToken));

        Assert.DoesNotContain("991234567", stored, StringComparison.Ordinal);
    }
}
