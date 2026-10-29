using Vitrio.Domain.Locations;

namespace Vitrio.Domain.Tests.Locations;

public sealed class BrazilTimeZonesTests
{
    [Theory]
    [InlineData("SP", "Jaboticabal", "America/Sao_Paulo")]
    [InlineData("PE", "Fernando de Noronha", "America/Noronha")]
    [InlineData("PE", "Recife", "America/Recife")]
    [InlineData("AM", "Eirunepé", "America/Eirunepe")]
    [InlineData("AM", "Manaus", "America/Manaus")]
    [InlineData("AC", "Rio Branco", "America/Rio_Branco")]
    [InlineData("MS", "Campo Grande", "America/Campo_Grande")]
    public void For_ReturnsTheZoneOfTheStateWithItsExceptions(string state, string city, string expected)
    {
        Assert.Equal(expected, BrazilTimeZones.For(state, city));
        Assert.NotNull(TimeZoneInfo.FindSystemTimeZoneById(expected));
    }
}
