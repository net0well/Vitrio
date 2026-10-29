using Mediator;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ResendCode;

public sealed record ResendCodeCommand(string Email, EmailCodePurpose Purpose) : ICommand<Result>;
