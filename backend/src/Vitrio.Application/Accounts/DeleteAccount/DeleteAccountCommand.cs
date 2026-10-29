using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.DeleteAccount;

public sealed record DeleteAccountCommand : ICommand<Result>;
