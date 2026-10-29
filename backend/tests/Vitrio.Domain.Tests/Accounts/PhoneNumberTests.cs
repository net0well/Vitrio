using Vitrio.Domain.Accounts;

namespace Vitrio.Domain.Tests.Accounts;

public sealed class PhoneNumberTests
{
    [Theory]
    [InlineData("(16) 99123-4567", "+5516991234567")]
    [InlineData("16991234567", "+5516991234567")]
    [InlineData("+55 11 98765-4321", "+5511987654321")]
    [InlineData("55 21 99999 0000", "+5521999990000")]
    public void Create_WithMobileNumber_ReturnsE164(string input, string expected)
    {
        Assert.Equal(expected, PhoneNumber.Create(input).Value.Value);
    }

    [Theory]
    [InlineData("(16) 3202-1234")]
    [InlineData("99123-4567")]
    [InlineData("(10) 99123-4567")]
    [InlineData("+1 415 555 0101")]
    [InlineData("")]
    [InlineData(null)]
    public void Create_WithLandlineMissingAreaCodeOrForeignNumber_Fails(string? input)
    {
        Assert.Equal(AccountErrors.InvalidPhoneNumber, PhoneNumber.Create(input).Error);
    }
}
