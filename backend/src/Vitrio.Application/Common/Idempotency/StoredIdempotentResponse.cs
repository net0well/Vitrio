namespace Vitrio.Application.Common.Idempotency;

public sealed record StoredIdempotentResponse(string RequestHash, string ResponseBody);
