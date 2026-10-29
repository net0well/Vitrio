using Vitrio.Domain.Common;

namespace Vitrio.Domain.Tests.Common;

public sealed class SearchTextTests
{
    [Theory]
    [InlineData("São José", "sao jose")]
    [InlineData("  SÃO   JOSÉ dos Campos ", "sao jose dos campos")]
    [InlineData("Itaú de Minas", "itau de minas")]
    [InlineData(null, "")]
    public void Normalize_RemovesAccentsCaseAndExtraSpaces(string? input, string expected)
    {
        Assert.Equal(expected, SearchText.Normalize(input));
    }
}
