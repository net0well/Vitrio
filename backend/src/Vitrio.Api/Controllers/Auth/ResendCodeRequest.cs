using System.Text.Json.Serialization;
using Vitrio.Application.Accounts.ResendCode;
using Vitrio.Domain.Accounts;

namespace Vitrio.Api.Controllers.Auth;

public sealed record ResendCodeRequest(string? Email, [property: JsonRequired] EmailCodePurpose Purpose)
{
    public ResendCodeCommand ToCommand() => new(Email ?? string.Empty, Purpose);
}
