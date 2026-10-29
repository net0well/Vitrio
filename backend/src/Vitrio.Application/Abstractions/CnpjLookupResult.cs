namespace Vitrio.Application.Abstractions;

public sealed record CnpjLookupResult(string? CompanyName, bool IsActive, bool IsMei);
