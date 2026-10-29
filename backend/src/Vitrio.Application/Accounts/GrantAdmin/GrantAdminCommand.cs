using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.GrantAdmin;

public sealed record GrantAdminCommand(string Email) : ICommand<Result>;
