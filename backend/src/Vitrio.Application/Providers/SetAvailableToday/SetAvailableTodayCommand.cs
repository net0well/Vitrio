using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Providers.SetAvailableToday;

public sealed record SetAvailableTodayCommand(bool Value) : ICommand<Result>;
