using Mediator;
using Vitrio.Shared.Results;

namespace Vitrio.Application.Accounts.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
