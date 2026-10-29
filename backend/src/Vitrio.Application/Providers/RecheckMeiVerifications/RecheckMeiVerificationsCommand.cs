using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Providers.RecheckMeiVerifications;

public sealed record RecheckMeiVerificationsCommand(int BatchSize, TimeSpan PauseBetweenLookups) : ICommand<Result>;
