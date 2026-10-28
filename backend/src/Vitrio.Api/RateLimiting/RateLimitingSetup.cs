using System.Globalization;
using System.Threading.RateLimiting;
using Vitrio.Api.Authentication;

namespace Vitrio.Api.RateLimiting;

public static class RateLimitingSetup
{
    public static IServiceCollection AddVitrioRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, _) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
                }

                return ValueTask.CompletedTask;
            };

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                IsReadOnly(context.Request.Method)
                    ? RateLimitPartition.GetNoLimiter(string.Empty)
                    : RateLimitPartition.Get($"write:{UserOrIp(context)}", _ => Window(60, TimeSpan.FromMinutes(1))));

            options.AddPolicy(RateLimitPolicies.Auth, context =>
                RateLimitPartition.Get(Ip(context), _ => new ChainedRateLimiter(
                    Window(10, TimeSpan.FromMinutes(1)),
                    Window(30, TimeSpan.FromHours(1)))));

            options.AddPolicy(RateLimitPolicies.Search, context =>
                RateLimitPartition.Get(UserOrIp(context), _ => Window(60, TimeSpan.FromMinutes(1))));

            options.AddPolicy(RateLimitPolicies.Profile, context =>
                RateLimitPartition.Get(UserOrIp(context), _ => Window(120, TimeSpan.FromMinutes(1))));

            options.AddPolicy(RateLimitPolicies.Orders, context =>
                RateLimitPartition.Get(UserOrIp(context), _ => new ChainedRateLimiter(
                    Window(10, TimeSpan.FromHours(1)),
                    Window(30, TimeSpan.FromDays(1)))));

            options.AddPolicy(RateLimitPolicies.Quotes, context =>
                RateLimitPartition.Get(UserOrIp(context), _ => Window(60, TimeSpan.FromHours(1))));

            options.AddPolicy(RateLimitPolicies.Reports, context =>
                RateLimitPartition.Get(UserOrIp(context), _ => Window(10, TimeSpan.FromDays(1))));

            options.AddPolicy(RateLimitPolicies.Uploads, context =>
                RateLimitPartition.Get(UserOrIp(context), _ => Window(60, TimeSpan.FromHours(1))));
        });

        return services;
    }

    private static FixedWindowRateLimiter Window(int permitLimit, TimeSpan window) => new(new FixedWindowRateLimiterOptions
    {
        PermitLimit = permitLimit,
        Window = window,
        QueueLimit = 0,
        AutoReplenishment = true
    });

    private static bool IsReadOnly(string method) =>
        HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method);

    private static string UserOrIp(HttpContext context) =>
        context.User.FindFirst(ClaimNames.Subject)?.Value is { } subject ? $"user:{subject}" : Ip(context);

    private static string Ip(HttpContext context) =>
        $"ip:{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}";
}
