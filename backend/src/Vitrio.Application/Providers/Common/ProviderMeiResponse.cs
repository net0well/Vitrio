using Vitrio.Domain.Providers;

namespace Vitrio.Application.Providers.Common;

public sealed record ProviderMeiResponse(string Cnpj, MeiStatus Status, string? CompanyName, string? Reason, DateTimeOffset CheckedAt);
