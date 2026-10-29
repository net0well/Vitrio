using Mediator;
using Vitrio.Application.Accounts.GrantAdmin;

namespace Vitrio.Api.Hosting;

public static class AdminCommand
{
    public const string GrantAdmin = "grant-admin";

    public static bool IsRequested(string[] args) => args is [GrantAdmin, _];

    public static async Task<int> RunAsync(IServiceProvider services, string[] args)
    {
        await using var scope = services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new GrantAdminCommand(args[1]));
        if (result.IsFailure)
        {
            await Console.Error.WriteLineAsync($"Could not grant admin: {result.Error.Code}");
            return 1;
        }

        await Console.Out.WriteLineAsync("Admin granted.");
        return 0;
    }
}
