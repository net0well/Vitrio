using System.Diagnostics;
using System.Net;
using System.Text.Json.Serialization;
using Asp.Versioning;
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
using Scalar.AspNetCore;
using Serilog;
using Vitrio.Api.Authentication;
using Vitrio.Api.Authorization;
using Vitrio.Api.Controllers;
using Vitrio.Api.Logging;
using Vitrio.Api.RateLimiting;
using Vitrio.Api.Security;
using Vitrio.Application;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Common;
using Vitrio.Application.Common.Behaviors;
using Vitrio.Infrastructure;

const int MaxRequestBodySize = 64 * 1024;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services
    .AddControllers()
    .AddJsonOptions(json =>
    {
        json.JsonSerializerOptions.UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow;
        json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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
    })
    .AddOpenApi();

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
    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
    {
        forwarded.KnownProxies.Add(IPAddress.Parse(proxy));
    }
});

builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration(JwtOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwt) =>
    {
        bearer.MapInboundClaims = false;
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Value.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Value.Audience,
            ValidateLifetime = true,
            RequireExpirationTime = true,
            ValidateIssuerSigningKey = true,
            RequireSignedTokens = true,
            ValidAlgorithms = [SecurityAlgorithms.EcdsaSha256],
            NameClaimType = ClaimNames.Subject,
            RoleClaimType = ClaimNames.Roles,
            ClockSkew = TimeSpan.FromSeconds(30)
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
        context => !context.Request.Path.StartsWithSegments("/scalar") && !context.Request.Path.StartsWithSegments("/openapi"),
        branch => branch.UseMiddleware<SecurityHeadersMiddleware>());
    app.MapOpenApi().WithDocumentPerVersion().AllowAnonymous();
    app.MapScalarApiReference().AllowAnonymous();
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

await app.RunAsync();
