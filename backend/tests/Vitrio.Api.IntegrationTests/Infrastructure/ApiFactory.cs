using System.Security.Cryptography;
using Mediator;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog.Core;
using Testcontainers.PostgreSql;
using Vitrio.Application.Abstractions;
using Vitrio.Application.Locations.LoadCities;
using Vitrio.Infrastructure.Persistence;

[assembly: AssemblyFixture(typeof(Vitrio.Api.IntegrationTests.Infrastructure.ApiFactory))]

namespace Vitrio.Api.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TightRateLimitPolicy = "probe-tight";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgis/postgis:17-3.6").Build();

    public string ConnectionString => _database.GetConnectionString();

    private static readonly string SigningKey = CreateSigningKey();
    private static readonly string CodeKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    public AdjustableTimeProvider Clock { get; } = new();

    public CapturingEmailSender Emails { get; } = new();

    public FakeIbgeCitySource Ibge { get; } = new();

    public CapturedLogs Logs { get; } = new();

    public async ValueTask InitializeAsync()
    {
        await _database.StartAsync();

        using var scope = Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(new LoadCitiesCommand(["SP", "SC"]));
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Database", ConnectionString);
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Auth:CodeKey", CodeKey);
        builder.ConfigureTestServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(ApiFactory).Assembly);
            services.AddSingleton<IStartupFilter, TestClientAddressStartupFilter>();
            services.Replace(ServiceDescriptor.Singleton<TimeProvider>(Clock));
            services.Replace(ServiceDescriptor.Singleton<IEmailSender>(Emails));
            services.Replace(ServiceDescriptor.Singleton<ICepLookup, FakeCepLookup>());
            services.Replace(ServiceDescriptor.Singleton<IIbgeCitySource>(Ibge));
            services.AddSingleton<ILogEventSink>(Logs);
            services.Configure<RateLimiterOptions>(options => options.AddFixedWindowLimiter(TightRateLimitPolicy, limiter =>
            {
                limiter.PermitLimit = 1;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            }));
        });
    }

    private static string CreateSigningKey()
    {
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        return key.ExportPkcs8PrivateKeyPem();
    }
}
