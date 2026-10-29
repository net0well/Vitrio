using Vitrio.Application.Abstractions;

namespace Vitrio.Api.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid Id => ReadGuid(ClaimNames.Subject);

    public Guid SessionId => ReadGuid(ClaimNames.SessionId);

    private Guid ReadGuid(string claim) =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(claim)?.Value, out var value)
            ? value
            : throw new InvalidOperationException("There is no authenticated user in the current request.");
}
