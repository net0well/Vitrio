using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.DeleteAddress;

public sealed record DeleteAddressCommand(Guid Id) : ICommand<Result>;
