namespace Vitrio.Api.Security;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;
            headers.XContentTypeOptions = "nosniff";
            headers.XFrameOptions = "DENY";
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";
            headers["Referrer-Policy"] = "no-referrer";

            if (context.User.Identity?.IsAuthenticated == true || context.Request.Headers.Authorization.Count > 0)
            {
                headers.CacheControl = "no-store";
            }

            return Task.CompletedTask;
        });

        return next(context);
    }
}
