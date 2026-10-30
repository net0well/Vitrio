namespace Vitrio.Domain.Common;

public readonly record struct Money(long Cents)
{
    public decimal Reais => Cents / 100m;

    public static Money? FromReais(decimal? reais) =>
        reais is { } value && decimal.Round(value, 2) == value ? new Money((long)(value * 100)) : null;
}
