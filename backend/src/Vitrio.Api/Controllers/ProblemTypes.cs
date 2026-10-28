namespace Vitrio.Api.Controllers;

public static class ProblemTypes
{
    public const string BaseUri = "https://vitrio.app/problems/";

    public static string For(string code) => BaseUri + code;

    public static string ForStatus(int statusCode) => For(statusCode switch
    {
        StatusCodes.Status400BadRequest => "validation",
        StatusCodes.Status401Unauthorized => "unauthorized",
        StatusCodes.Status403Forbidden => "forbidden",
        StatusCodes.Status404NotFound => "not_found",
        StatusCodes.Status405MethodNotAllowed => "method_not_allowed",
        StatusCodes.Status406NotAcceptable => "not_acceptable",
        StatusCodes.Status409Conflict => "conflict",
        StatusCodes.Status413PayloadTooLarge => "payload_too_large",
        StatusCodes.Status415UnsupportedMediaType => "unsupported_media_type",
        StatusCodes.Status422UnprocessableEntity => "unprocessable",
        StatusCodes.Status429TooManyRequests => "rate_limited",
        StatusCodes.Status503ServiceUnavailable => "unavailable",
        >= 500 => "internal_error",
        _ => $"http_{statusCode}"
    });
}
