using Microsoft.AspNetCore.Authentication.JwtBearer;
using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.Authentication;

namespace Vitrio.Api.Authentication;

internal sealed class SessionValidationEvents(ISessionStatusChecker sessions) : JwtBearerEvents
{
    public override async Task TokenValidated(TokenValidatedContext context)
    {
        if (!Guid.TryParse(context.Principal?.FindFirst(ClaimNames.SessionId)?.Value, out var sessionId)
            || !await sessions.IsActiveAsync(sessionId, context.HttpContext.RequestAborted))
        {
            context.Fail("The session is no longer active.");
        }
    }
}
