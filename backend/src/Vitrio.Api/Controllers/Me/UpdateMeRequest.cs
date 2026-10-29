using Vitrio.Application.Accounts.UpdateMe;

namespace Vitrio.Api.Controllers.Me;

public sealed record UpdateMeRequest(string? DisplayName, string? PhoneNumber)
{
    public UpdateMeCommand ToCommand() => new(DisplayName, PhoneNumber);
}
