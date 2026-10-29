using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.DeleteAddress;

public sealed record DeleteAddressCommand(Guid Id) : ICommand<Result>;
