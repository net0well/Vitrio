using System.Globalization;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Vitrio.Shared.Results;

namespace Vitrio.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private const string ProblemContentType = "application/problem+json";

    protected IActionResult ToActionResult(Result result) =>
        result.IsSuccess ? NoContent() : ErrorResult(result.Error);

    protected IActionResult ToActionResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : ErrorResult(result.Error);

    protected IActionResult ToCreatedResult<T>(Result<T> result) =>
        result.IsSuccess ? StatusCode(StatusCodes.Status201Created, result.Value) : ErrorResult(result.Error);

    protected IActionResult ToAcceptedResult(Result result) =>
        result.IsSuccess ? StatusCode(StatusCodes.Status202Accepted) : ErrorResult(result.Error);

    protected ObjectResult ErrorResult(Error error)
    {
        var statusCode = StatusCodeFor(error.Type);
        var type = ProblemTypes.For(error.Code);

        ProblemDetails problem;
        if (error is ValidationError validation)
        {
            var modelState = new ModelStateDictionary();
            foreach (var (field, messages) in validation.Errors)
            {
                foreach (var message in messages)
                {
                    modelState.AddModelError(ToJsonPath(field), message);
                }
            }

            problem = ProblemDetailsFactory.CreateValidationProblemDetails(
                HttpContext, modelState, statusCode, type: type, detail: error.Message);
        }
        else
        {
            problem = ProblemDetailsFactory.CreateProblemDetails(
                HttpContext, statusCode, type: type, detail: error.Message);
        }

        if (error is MissingFieldsError incomplete)
        {
            problem.Extensions["missing"] = incomplete.Missing;
        }

        if (error.RetryAfter is { } retryAfter)
        {
            Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        return new ObjectResult(problem)
        {
            StatusCode = statusCode,
            ContentTypes = { ProblemContentType }
        };
    }

    private static int StatusCodeFor(ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Unprocessable => StatusCodes.Status422UnprocessableEntity,
        ErrorType.RateLimited => StatusCodes.Status429TooManyRequests,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string ToJsonPath(string propertyPath) =>
        string.Join('.', propertyPath.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
