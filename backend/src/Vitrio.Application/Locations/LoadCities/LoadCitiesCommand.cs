using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Locations.LoadCities;

public sealed record LoadCitiesCommand(IReadOnlyList<string>? States = null) : ICommand<Result<int>>;
