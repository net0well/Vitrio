using Vitrio.Domain.Providers;

namespace Vitrio.Domain.Tests.Providers;

public sealed class CnpjTests
{
    [Theory]
    [InlineData("11.222.333/0001-81", "11222333000181")]
    [InlineData("11222333000181", "11222333000181")]
    public void Create_WithValidCheckDigits_KeepsOnlyTheDigits(string input, string expected)
    {
        Assert.Equal(expected, Cnpj.Create(input).Value.Value);
    }

    [Theory]
    [InlineData("11.222.333/0001-82")]
    [InlineData("11111111111111")]
    [InlineData("1122233300018")]
    [InlineData(null)]
    public void Create_WithInvalidInput_Fails(string? input)
    {
        Assert.Equal(ProviderErrors.InvalidCnpj, Cnpj.Create(input).Error);
    }
}
