using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.SetAvailableToday;

public sealed record SetAvailableTodayCommand(bool Value) : ICommand<Result>;
