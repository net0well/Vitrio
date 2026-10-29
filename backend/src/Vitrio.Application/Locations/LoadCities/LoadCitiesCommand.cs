using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Locations.LoadCities;

public sealed record LoadCitiesCommand(IReadOnlyList<string>? States = null) : ICommand<Result<int>>;
