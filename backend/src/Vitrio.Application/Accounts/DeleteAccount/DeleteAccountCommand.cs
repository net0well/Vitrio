using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.DeleteAccount;

public sealed record DeleteAccountCommand : ICommand<Result>;
