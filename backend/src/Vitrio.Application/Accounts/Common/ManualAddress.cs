namespace Vitrio.Application.Accounts.Common;

public sealed record ManualAddress(string? Street, string? Neighborhood, string? State, int? CityIbgeCode)
{
    public bool IsEmpty => Street is null && Neighborhood is null && State is null && CityIbgeCode is null;
}
