using Vitrio.Application.Accounts.VerifyEmail;

namespace Vitrio.Application.Tests.Accounts;

public sealed class VerifyEmailCommandValidatorTests
{
    private readonly VerifyEmailCommandValidator _validator = new();

    [Fact]
    public void Validate_WithSixDigits_Passes()
    {
        Assert.True(_validator.Validate(new VerifyEmailCommand("ana@exemplo.com.br", "048291", "vidro azul na janela", null)).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("48291")]
    [InlineData("4829170")]
    [InlineData("48 291")]
    [InlineData("abcdef")]
    public void Validate_WithoutSixDigits_ReportsCode(string code)
    {
        var result = _validator.Validate(new VerifyEmailCommand("ana@exemplo.com.br", code, "vidro azul na janela", null));

        Assert.Equal(nameof(VerifyEmailCommand.Code), Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_WithoutPassword_ReportsPassword()
    {
        var result = _validator.Validate(new VerifyEmailCommand("ana@exemplo.com.br", "048291", "", null));

        Assert.Equal(nameof(VerifyEmailCommand.Password), Assert.Single(result.Errors).PropertyName);
    }
}
