using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record MeiCheck(MeiCheckOutcome Outcome, string? CompanyName);
