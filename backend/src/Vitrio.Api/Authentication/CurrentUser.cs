using Vitrio.Application.Abstractions;
using Vitrio.Infrastructure.Authentication;

namespace Vitrio.Api.Authentication;

internal sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public Guid Id =>
        Guid.TryParse(httpContextAccessor.HttpContext?.User.FindFirst(ClaimNames.Subject)?.Value, out var id)
            ? id
            : throw new InvalidOperationException("There is no authenticated user in the current request.");
}
