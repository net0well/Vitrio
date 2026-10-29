using Vitrio.Application.Accounts.Common;

namespace Vitrio.Application.Tests.Accounts;

public sealed class PasswordPolicyTests
{
    [Theory]
    [InlineData("vidro azul")]
    [InlineData("janela-de-vidro-2026")]
    [InlineData("pão de queijo quente")]
    public void FindProblem_WithLongUncommonPassword_ReturnsNothing(string password)
    {
        Assert.Null(PasswordPolicy.FindProblem(password, "ana@exemplo.com.br"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("curta1234")]
    public void FindProblem_WithLessThanTenCharacters_ReturnsTooShort(string? password)
    {
        Assert.Equal(PasswordProblem.TooShort, PasswordPolicy.FindProblem(password, "ana@exemplo.com.br"));
    }

    [Fact]
    public void FindProblem_CountsEmojiAsOneCharacter()
    {
        Assert.Equal(PasswordProblem.TooShort, PasswordPolicy.FindProblem("vidro👩‍👩‍👧‍👦ok", null));
    }

    [Fact]
    public void FindProblem_MeasuresTheNormalizedPassword()
    {
        Assert.Null(PasswordPolicy.FindProblem("ｖｉｄｒｏ ａｚｕｌ", null));
        Assert.Equal("vidro azul", PasswordPolicy.Normalize("ｖｉｄｒｏ ａｚｕｌ"));
    }

    [Fact]
    public void FindProblem_WithMoreThan128Characters_ReturnsTooLong()
    {
        Assert.Equal(PasswordProblem.TooLong, PasswordPolicy.FindProblem(new string('v', PasswordPolicy.MaxLength + 1), null));
        Assert.Null(PasswordPolicy.FindProblem(new string('v', PasswordPolicy.MaxLength - 1) + "z", null));
    }

    [Theory]
    [InlineData("1234567890")]
    [InlineData("qwertyuiop")]
    [InlineData("Senha12345")]
    [InlineData("FLAMENGO123")]
    public void FindProblem_WithCommonPassword_ReturnsCommon(string password)
    {
        Assert.Equal(PasswordProblem.Common, PasswordPolicy.FindProblem(password, null));
    }

    [Fact]
    public void FindProblem_WithPasswordEqualToEmail_ReturnsSameAsEmail()
    {
        Assert.Equal(PasswordProblem.SameAsEmail, PasswordPolicy.FindProblem("Ana.Lima@Exemplo.com.br", "ana.lima@exemplo.com.br"));
    }
}
