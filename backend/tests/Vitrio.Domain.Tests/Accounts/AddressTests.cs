using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class AddressTests
{
    private static readonly Cep Cep = Vitrio.Domain.Accounts.Cep.Create("14870-000").Value;

    private readonly FixedClock _clock = new(new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void Create_WithLocationAndNumber_KeepsTheNormalizedFields()
    {
        var address = Address.Create(Guid.CreateVersion7(), "  Casa ", Cep, Location(), "120", " ap 31 ", _clock).Value;

        Assert.Equal("Casa", address.Label);
        Assert.Equal("14870000", address.Cep);
        Assert.Equal("Rua Rui Barbosa", address.Street);
        Assert.Equal("ap 31", address.Complement);
        Assert.Equal(3524303, address.CityIbgeCode);
    }

    [Theory]
    [InlineData("", "120", null)]
    [InlineData("Casa", "", null)]
    [InlineData("Ligue 16 99123-4567", "120", null)]
    [InlineData("Casa", "120", "chama no www.exemplo.com")]
    public void Create_WithInvalidLabelNumberOrComplement_Fails(string label, string number, string? complement)
    {
        Assert.True(Address.Create(Guid.CreateVersion7(), label, Cep, Location(), number, complement, _clock).IsFailure);
    }

    [Theory]
    [InlineData("", "Centro", "Jaboticabal", "SP", 3524303)]
    [InlineData("Rua A", "", "Jaboticabal", "SP", 3524303)]
    [InlineData("Rua A", "Centro", "Jaboticabal", "XX", 3524303)]
    [InlineData("Rua A", "Centro", "Jaboticabal", "SP", 12)]
    public void PostalLocation_WithMissingStreetNeighborhoodOrCity_Fails(string street, string neighborhood, string city, string state, int code)
    {
        Assert.True(PostalLocation.Create(street, neighborhood, city, state, code, null, null).IsFailure);
    }

    [Theory]
    [InlineData("14870-000", "14870000")]
    [InlineData("14.870-000", "14870000")]
    public void Cep_AcceptsTheUsualFormats(string input, string expected)
    {
        Assert.Equal(expected, Vitrio.Domain.Accounts.Cep.Create(input).Value.Value);
    }

    [Theory]
    [InlineData("1487000")]
    [InlineData("00000000")]
    [InlineData("abcdefgh")]
    public void Cep_WithoutEightDigits_Fails(string input)
    {
        Assert.Equal(AccountErrors.InvalidCep, Vitrio.Domain.Accounts.Cep.Create(input).Error);
    }

    private static PostalLocation Location() =>
        PostalLocation.Create("Rua Rui Barbosa", "Centro", "Jaboticabal", "SP", 3524303, -21.25, -48.32).Value;

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }
}
