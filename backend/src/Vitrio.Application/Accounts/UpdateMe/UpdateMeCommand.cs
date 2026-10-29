using Mediator;
using Vitrio.Application.Accounts.Common;
using Vitrio.Domain.Common;

namespace Vitrio.Application.Accounts.UpdateMe;

public sealed record UpdateMeCommand(string? DisplayName, string? PhoneNumber) : ICommand<Result<MeResponse>>;
