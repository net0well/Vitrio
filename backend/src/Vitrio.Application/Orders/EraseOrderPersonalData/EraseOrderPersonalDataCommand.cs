using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Orders.EraseOrderPersonalData;

public sealed record EraseOrderPersonalDataCommand(int BatchSize) : ICommand<Result>;
