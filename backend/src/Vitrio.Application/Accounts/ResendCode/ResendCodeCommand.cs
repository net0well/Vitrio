using Mediator;
using Vitrio.Domain.Accounts;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ResendCode;

public sealed record ResendCodeCommand(string Email, EmailCodePurpose Purpose) : ICommand<Result>;
