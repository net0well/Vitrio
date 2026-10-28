using Vitrio.Domain.Accounts;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class DisplayNameTests
{
    [Fact]
    public void Create_TrimsAndCollapsesSpaces()
    {
        var name = DisplayName.Create("  Mariana   Ferreira\tSouza ");

        Assert.True(name.IsSuccess);
        Assert.Equal("Mariana Ferreira Souza", name.Value.Value);
    }

    [Theory]
    [InlineData("Mariana Ferreira Souza", "Mariana F.", "MF")]
    [InlineData("Carlos", "Carlos", "C")]
    [InlineData("ana luiza", "ana L.", "AL")]
    [InlineData("Mariana da Silva", "Mariana S.", "MS")]
    [InlineData("João dos Santos", "João S.", "JS")]
    public void Create_ExposesShortFormsForOtherPeople(string input, string firstNameWithInitial, string initials)
    {
        var name = DisplayName.Create(input).Value;

        Assert.Equal(firstNameWithInitial, name.FirstNameWithInitial);
        Assert.Equal(initials, name.Initials);
        Assert.Equal(input.Split(' ')[0], name.FirstName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("A")]
    [InlineData("   ")]
    public void Create_WithTooShortName_Fails(string? input)
    {
        Assert.Equal(AccountErrors.InvalidDisplayName, DisplayName.Create(input).Error);
    }

    [Fact]
    public void Create_WithMoreThan60Characters_Fails()
    {
        Assert.Equal(AccountErrors.InvalidDisplayName, DisplayName.Create(new string('a', 61)).Error);
    }

    [Theory]
    [InlineData("Carlos 16 99123-4567")]
    [InlineData("Carlos carlos@example.com")]
    [InlineData("Carlos www.carlos.com.br")]
    [InlineData("Carlos https://carlos.dev")]
    public void Create_WithContactInformation_Fails(string input)
    {
        Assert.Equal(AccountErrors.DisplayNameWithContact, DisplayName.Create(input).Error);
    }

    [Fact]
    public void Create_RemovesControlCharacters()
    {
        Assert.Equal("Mariana", DisplayName.Create("Mari\u0000ana​").Value.Value);
    }
}
