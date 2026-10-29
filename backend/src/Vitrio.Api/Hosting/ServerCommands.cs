using Mediator;
using Vitrio.Application.Accounts.GrantAdmin;
using Vitrio.Application.Locations.LoadCities;

namespace Vitrio.Api.Hosting;

public static class ServerCommands
{
    public const string GrantAdmin = "grant-admin";
    public const string LoadCities = "load-cities";

    public static bool IsRequested(string[] args) => args is [GrantAdmin, _] or [LoadCities, ..];

    public static async Task<int> RunAsync(IServiceProvider services, string[] args, CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        if (args is [GrantAdmin, var email])
        {
            var granted = await sender.Send(new GrantAdminCommand(email), cancellationToken);
            return await ReportAsync(granted.IsSuccess, "Admin granted.", granted.Error?.Code);
        }

        var states = args.Skip(1).Select(state => state.ToUpperInvariant()).ToList();
        var loaded = await sender.Send(new LoadCitiesCommand(states.Count == 0 ? null : states), cancellationToken);
        return await ReportAsync(loaded.IsSuccess, loaded.IsSuccess ? $"{loaded.Value} cities loaded." : string.Empty, loaded.Error?.Code);
    }

    private static async Task<int> ReportAsync(bool succeeded, string message, string? errorCode)
    {
        if (!succeeded)
        {
            await Console.Error.WriteLineAsync($"Command failed: {errorCode}");
            return 1;
        }

        await Console.Out.WriteLineAsync(message);
        return 0;
    }
}
