using Vitrio.Application.Providers.SetAvailableToday;

namespace Vitrio.Api.Controllers.Provider;

public sealed record AvailableTodayRequest(bool? Value)
{
    public SetAvailableTodayCommand ToCommand() => new(Value ?? false);
}
