using System.Net;
using System.Net.Http.Json;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Accounts.Common;
using Vitrio.Application.Accounts.GrantAdmin;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.IntegrationTests.Accounts;

public sealed class AccountDeletionTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateAuthClient();

    [Fact]
    public async Task DeleteAccount_EndsTheSessionsAndRemovesPersonalData()
    {
        var email = AuthApi.NewEmail();
        var user = await factory.SeedActiveUserAsync(email);
        var auth = await (await _client.LoginAsync(email)).ReadAuthAsync();
        await _client.SendJsonAsAsync(HttpMethod.Patch, "/api/v1/me", auth.AccessToken, new { phoneNumber = "16991234567" });
        await _client.SendJsonAsAsync(HttpMethod.Post, "/api/v1/me/addresses", auth.AccessToken, new { label = "Casa", cep = FakeCepLookup.Jaboticabal, number = "120" });
        await _client.SendJsonAsAsync(HttpMethod.Put, $"/api/v1/me/devices/{Guid.NewGuid()}", auth.AccessToken, new { platform = "android", pushToken = "token" });

        var response = await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", auth.AccessToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", auth.AccessToken)).StatusCode);
        await (await _client.RefreshAsync(auth.RefreshToken)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_refresh_token");
        await (await _client.LoginAsync(email)).AssertProblemAsync(HttpStatusCode.Unauthorized, "auth.invalid_credentials");

        var stored = await factory.QueryAsync(db => db.Users.AsNoTracking().SingleAsync(account => account.Id == user.Id, TestContext.Current.CancellationToken));
        Assert.Equal(UserStatus.Deleted, stored.Status);
        Assert.NotEqual(email, stored.Email);
        Assert.Equal(User.RemovedDisplayName, stored.DisplayName);
        Assert.Null(stored.PhoneNumber);
        Assert.False(await factory.QueryAsync(db => db.Addresses.AnyAsync(address => address.UserId == user.Id, TestContext.Current.CancellationToken)));
        Assert.False(await factory.QueryAsync(db => db.Devices.AnyAsync(device => device.UserId == user.Id, TestContext.Current.CancellationToken)));
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            entry => entry.TargetId == user.Id && entry.Action == AuditActions.AccountDeleted,
            TestContext.Current.CancellationToken)));
        var deletedEvents = await factory.QueryAsync(db => db.OutboxMessages
            .Where(message => message.Type == "user.deleted")
            .Select(message => message.Payload)
            .ToListAsync(TestContext.Current.CancellationToken));
        Assert.Contains(deletedEvents, payload => payload.Contains(user.Id.ToString(), StringComparison.Ordinal));
    }

    [Fact]
    public async Task DeleteAccount_FreesTheEmailForANewAccount()
    {
        var email = AuthApi.NewEmail();
        var user = await factory.SeedActiveUserAsync(email);
        var auth = await (await _client.LoginAsync(email)).ReadAuthAsync();
        await _client.SendAsAsync(HttpMethod.Delete, "/api/v1/me", auth.AccessToken);

        var register = await factory.CreateAuthClient().RegisterAsync(email);

        Assert.Equal(HttpStatusCode.Accepted, register.StatusCode);
        var newAccount = await factory.FindUserAsync(email);
        Assert.NotEqual(user.Id, newAccount!.Id);
        Assert.Equal(UserStatus.PendingVerification, newAccount.Status);
    }

    [Fact]
    public async Task DeleteAccount_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/v1/me"), TestContext.Current.CancellationToken);

        await response.AssertProblemAsync(HttpStatusCode.Unauthorized, "unauthorized");
    }

    [Fact]
    public async Task GrantAdmin_AddsTheAdminRoleToTheTokenAndToMe()
    {
        var email = AuthApi.NewEmail();
        var user = await factory.SeedActiveUserAsync(email);

        var result = await SendAsync(new GrantAdminCommand(email));

        Assert.True(result.IsSuccess);
        var auth = await (await _client.LoginAsync(email)).ReadAuthAsync();
        Assert.Contains(Roles.Admin, auth.User.Roles);
        var me = await (await _client.SendAsAsync(HttpMethod.Get, "/api/v1/me", auth.AccessToken))
            .Content.ReadFromJsonAsync<MeResponse>(TestContext.Current.CancellationToken);
        Assert.Equal(new[] { Roles.Customer, Roles.Admin }, me!.Roles);
        Assert.True(await factory.QueryAsync(db => db.AuditEvents.AnyAsync(
            entry => entry.TargetId == user.Id && entry.Action == AuditActions.AdminGranted,
            TestContext.Current.CancellationToken)));
        Assert.True((await SendAsync(new GrantAdminCommand(email))).IsSuccess);
    }

    [Fact]
    public async Task GrantAdmin_ForUnknownEmail_Fails()
    {
        var result = await SendAsync(new GrantAdminCommand(AuthApi.NewEmail()));

        Assert.Equal(AccountErrors.AccountNotFound, result.Error);
    }

    private async Task<Vitrio.Domain.Common.Result> SendAsync(GrantAdminCommand command)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISender>().Send(command, TestContext.Current.CancellationToken);
    }
}
