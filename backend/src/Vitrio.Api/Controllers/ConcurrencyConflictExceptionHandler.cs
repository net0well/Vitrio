using Microsoft.AspNetCore.Diagnostics;
using Vitrio.Application.Abstractions;

namespace Vitrio.Api.Controllers;

internal sealed class ConcurrencyConflictExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public const string Code = "conflict";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ConcurrencyConflictException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails =
            {
                Status = StatusCodes.Status409Conflict,
                Type = ProblemTypes.For(Code),
                Detail = "Os dados mudaram enquanto você fazia isso. Atualize e tente de novo."
            }
        });
    }
}
