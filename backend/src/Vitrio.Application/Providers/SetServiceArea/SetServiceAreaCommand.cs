using Mediator;
using Vitrio.Application.Providers.Common;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.SetServiceArea;

public sealed record SetServiceAreaCommand(
    string Cep,
    string Number,
    string? Complement,
    int RadiusKm,
    IReadOnlyList<string> Neighborhoods) : ICommand<Result<ServiceAreaSavedResponse>>;
