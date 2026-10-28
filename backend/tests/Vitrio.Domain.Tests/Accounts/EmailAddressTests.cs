using Vitrio.Domain.Accounts;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class EmailAddressTests
{
    [Fact]
    public void Create_NormalizesCaseAndSpaces()
    {
        var email = EmailAddress.Create("  Mariana.Souza@Example.COM ");

        Assert.True(email.IsSuccess);
        Assert.Equal("mariana.souza@example.com", email.Value.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("mariana")]
    [InlineData("mariana@")]
    [InlineData("@example.com")]
    [InlineData("mariana@example")]
    [InlineData("mari ana@example.com")]
    [InlineData("mariana@@example.com")]
    public void Create_WithInvalidAddress_Fails(string? input)
    {
        var email = EmailAddress.Create(input);

        Assert.True(email.IsFailure);
        Assert.Equal(AccountErrors.InvalidEmail, email.Error);
    }

    [Fact]
    public void Create_WithAddressLongerThan254Characters_Fails()
    {
        var email = EmailAddress.Create(new string('a', 64) + "@" + new string('b', 186) + ".com");

        Assert.True(email.IsFailure);
    }
}
