using Mediator;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
