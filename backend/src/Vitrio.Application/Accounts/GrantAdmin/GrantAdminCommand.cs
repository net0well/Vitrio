using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.GrantAdmin;

public sealed record GrantAdminCommand(string Email) : ICommand<Result>;
