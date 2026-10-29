using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Vitrio.Api.Auditing;
using Vitrio.Api.Authentication;
using Vitrio.Api.Authorization;
using Vitrio.Api.Controllers;
using Vitrio.Api.Documentation;
using Vitrio.Api.Hosting;
using Vitrio.Api.Logging;
using Vitrio.Api.RateLimiting;
using Vitrio.Api.Security;
using Vitrio.Application;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common;
using Vitrio.Application.Common.Behaviors;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Infrastructure;
using Vitrio.Infrastructure.Authentication;

const int MaxRequestBodySize = 64 * 1024;

var builder = WebApplication.CreateBuilder(AdminCommand.IsRequested(args) ? [] : args);

if (builder.Environment.IsProduction())
{
    ProductionSettings.EnsureComplete(builder.Configuration);
}

builder.WebHost.ConfigureKestrel(kestrel =>
{
    kestrel.AddServerHeader = false;
    kestrel.Limits.MaxRequestBodySize = MaxRequestBodySize;
});

builder.Services.AddSerilog((services, logger) => logger
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.With<SensitiveDataMaskingEnricher>());

var openTelemetry = builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("vitrio-api"))
    .WithTracing(tracing => tracing
        .AddSource(ApplicationTelemetry.SourceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddNpgsql())
    .WithMetrics(metrics => metrics
        .AddMeter("Vitrio")
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation());

if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
{
    openTelemetry.UseOtlpExporter();
}

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediator((Mediator.MediatorOptions options) =>
{
    options.ServiceLifetime = ServiceLifetime.Scoped;
    options.Assemblies = [typeof(Vitrio.Application.DependencyInjection)];
    options.PipelineBehaviors = [typeof(LoggingBehavior<,>), typeof(ValidationBehavior<,>)];
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddScoped<IAuditLog, HttpAuditLog>();
builder.Services.AddScoped<SessionValidationEvents>();

builder.Services
    .AddControllers()
    .AddJsonOptions(json =>
    {
        json.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services
    .AddApiVersioning(versioning =>
    {
        versioning.DefaultApiVersion = new ApiVersion(1);
        versioning.ReportApiVersions = true;
        versioning.ApiVersionReader = new UrlSegmentApiVersionReader();
    })
    .AddMvc()
    .AddApiExplorer(explorer =>
    {
        explorer.GroupNameFormat = "'v'V";
        explorer.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<SwaggerDocuments>();

builder.Services.AddExceptionHandler<ConcurrencyConflictExceptionHandler>();
builder.Services.AddProblemDetails(problemDetails => problemDetails.CustomizeProblemDetails = context =>
{
    var problem = context.ProblemDetails;
    if (problem.Type is null || !problem.Type.StartsWith(ProblemTypes.BaseUri, StringComparison.Ordinal))
    {
        problem.Type = ProblemTypes.ForStatus(problem.Status ?? context.HttpContext.Response.StatusCode);
    }

    problem.Extensions.TryAdd("traceId", Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
});

builder.Services.Configure<ForwardedHeadersOptions>(forwarded =>
{
    forwarded.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    foreach (var proxy in ProductionSettings.ReadList(builder.Configuration, "ForwardedHeaders:KnownProxies"))
    {
        forwarded.KnownProxies.Add(IPAddress.Parse(proxy));
    }

    foreach (var network in ProductionSettings.ReadList(builder.Configuration, "ForwardedHeaders:KnownNetworks"))
    {
        forwarded.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
    }
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>, JwtSigningKey, IClock>((bearer, jwt, signingKey, clock) =>
    {
        bearer.MapInboundClaims = false;
        bearer.EventsType = typeof(SessionValidationEvents);
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey.Key,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.EcdsaSha256],
            NameClaimType = ClaimNames.Subject,
            RoleClaimType = ClaimNames.Roles,
            ClockSkew = TimeSpan.FromSeconds(30),
            LifetimeValidator = (notBefore, expires, _, parameters) =>
            {
                var now = clock.UtcNow.UtcDateTime;
                return expires is { } expiresAt
                    && expiresAt + parameters.ClockSkew > now
                    && (notBefore is not { } notBeforeAt || notBeforeAt - parameters.ClockSkew <= now);
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build())
    .AddPolicy(Policies.Customer, policy => policy.RequireRole(Roles.Customer))
    .AddPolicy(Policies.Provider, policy => policy.RequireRole(Roles.Provider))
    .AddPolicy(Policies.Admin, policy => policy.RequireRole(Roles.Admin));

builder.Services.AddVitrioRateLimiting();

var app = builder.Build();

app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseWhen(
        context => !context.Request.Path.StartsWithSegments("/swagger"),
        branch => branch.UseMiddleware<SecurityHeadersMiddleware>());
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        foreach (var version in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
        {
            ui.SwaggerEndpoint($"/swagger/{version.GroupName}/swagger.json", $"Vitrio API {version.GroupName}");
        }
    });
}
else
{
    app.UseMiddleware<SecurityHeadersMiddleware>();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false }).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains(Vitrio.Infrastructure.DependencyInjection.ReadinessTag)
}).AllowAnonymous();

if (AdminCommand.IsRequested(args))
{
    Environment.ExitCode = await AdminCommand.RunAsync(app.Services, args, app.Lifetime.ApplicationStopping);
    return;
}

await app.RunAsync();
