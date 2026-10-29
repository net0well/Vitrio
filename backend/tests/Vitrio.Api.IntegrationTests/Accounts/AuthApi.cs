using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Vitrio.Api.IntegrationTests.Infrastructure;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure.Persistence;

namespace Vitrio.Api.IntegrationTests.Accounts;

internal static class AuthApi
{
    public const string Password = "vidro azul na janela";
    public const string TermsVersion = "2026-10-28";

    public static string NewEmail() => $"pessoa-{Guid.CreateVersion7():N}@exemplo.com.br";

    public static HttpClient CreateAuthClient(this ApiFactory factory) => factory.CreateClient().WithOwnAddress();

    public static Task<HttpResponseMessage> RegisterAsync(
        this HttpClient client,
        string email,
        string displayName = "Ana Lima",
        string password = Password,
        string? acceptedTermsVersion = TermsVersion) =>
        client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new { displayName, email, password, acceptedTermsVersion },
            TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> VerifyEmailAsync(
        this HttpClient client,
        string email,
        string code,
        string password = Password,
        string deviceName = "Pixel 8") =>
        client.PostAsJsonAsync(
            "/api/v1/auth/verify-email",
            new { email, code, password, deviceName },
            TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> ResendCodeAsync(this HttpClient client, string email, string purpose = "verifyEmail") =>
        client.PostAsJsonAsync("/api/v1/auth/resend-code", new { email, purpose }, TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> LoginAsync(
        this HttpClient client,
        string email,
        string password = Password,
        string deviceName = "Pixel 8") =>
        client.PostAsJsonAsync("/api/v1/auth/login", new { email, password, deviceName }, TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> ForgotPasswordAsync(this HttpClient client, string email) =>
        client.PostAsJsonAsync("/api/v1/auth/password/forgot", new { email }, TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> ResetPasswordAsync(
        this HttpClient client,
        string email,
        string code,
        string newPassword,
        string deviceName = "Pixel 8") =>
        client.PostAsJsonAsync(
            "/api/v1/auth/password/reset",
            new { email, code, newPassword, deviceName },
            TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> ChangePasswordAsync(
        this HttpClient client,
        string accessToken,
        string currentPassword,
        string newPassword) =>
        client.SendAsync(
            WithBearer(
                new HttpRequestMessage(HttpMethod.Put, "/api/v1/me/password")
                {
                    Content = JsonContent.Create(new { currentPassword, newPassword })
                },
                accessToken),
            TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> RefreshAsync(this HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync("/api/v1/auth/refresh", new { refreshToken }, TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> LogoutAsync(this HttpClient client, string accessToken, bool allDevices) =>
        client.SendAsync(
            WithBearer(new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout") { Content = JsonContent.Create(new { allDevices }) }, accessToken),
            TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> SendJsonAsAsync(
        this HttpClient client,
        HttpMethod method,
        string url,
        string accessToken,
        object body) =>
        client.SendAsync(WithBearer(new HttpRequestMessage(method, url) { Content = JsonContent.Create(body) }, accessToken), TestContext.Current.CancellationToken);

    public static Task<HttpResponseMessage> SendAsAsync(this HttpClient client, HttpMethod method, string url, string accessToken) =>
        client.SendAsync(WithBearer(new HttpRequestMessage(method, url), accessToken), TestContext.Current.CancellationToken);

    public static async Task<AuthResponse> SignInNewAccountAsync(this ApiFactory factory, HttpClient client, string deviceName = "Pixel 8")
    {
        var email = NewEmail();
        await factory.SeedActiveUserAsync(email);
        return await (await client.LoginAsync(email, deviceName: deviceName)).ReadAuthAsync();
    }

    public static async Task<TokenResponse> ReadTokensAsync(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<TokenResponse>(TestContext.Current.CancellationToken))!;

    public static async Task<AuthResponse> ReadAuthAsync(this HttpResponseMessage response) =>
        (await response.Content.ReadFromJsonAsync<AuthResponse>(TestContext.Current.CancellationToken))!;

    private static HttpRequestMessage WithBearer(HttpRequestMessage request, string accessToken)
    {
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        return request;
    }

    public static async Task<User> SeedActiveUserAsync(this ApiFactory factory, string email, string displayName = "Bruno Costa")
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var user = User.Register(
            EmailAddress.Create(email).Value,
            DisplayName.Create(displayName).Value,
            hasher.Hash(Password),
            TermsVersion,
            clock);
        user.ConfirmEmail(clock);

        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(user);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);
        return user;
    }

    public static async Task<T> QueryAsync<T>(this ApiFactory factory, Func<AppDbContext, Task<T>> query)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    public static Task<User?> FindUserAsync(this ApiFactory factory, string email) =>
        factory.QueryAsync(db => db.Users.AsNoTracking().SingleOrDefaultAsync(
            user => user.Email == email,
            TestContext.Current.CancellationToken));
}
