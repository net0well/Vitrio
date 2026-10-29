namespace Vitrio.Domain.Providers;

public sealed record BaseAddress(string Cep, string? Street, string Number, string? Complement, string? Neighborhood, int CityIbgeCode)
{
    public const int NumberMaxLength = 10;
    public const int ComplementMaxLength = 60;
}
