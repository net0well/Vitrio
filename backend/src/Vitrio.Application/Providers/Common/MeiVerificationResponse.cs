using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record MeiVerificationResponse(MeiStatus Status, string? CompanyName, string? Reason);
