using Vitrio.Application.Accounts.Register;

namespace Vitrio.Application.Tests.Accounts;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_WithCompleteRegistration_Passes()
    {
        var result = _validator.Validate(Valid());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ReportsEmail()
    {
        var result = _validator.Validate(Valid() with { Email = "ana.exemplo.com.br" });

        Assert.Equal(nameof(RegisterCommand.Email), Assert.Single(result.Errors).PropertyName);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Ana ana@exemplo.com.br")]
    public void Validate_WithInvalidDisplayName_ReportsDisplayName(string displayName)
    {
        var result = _validator.Validate(Valid() with { DisplayName = displayName });

        Assert.Equal(nameof(RegisterCommand.DisplayName), Assert.Single(result.Errors).PropertyName);
    }

    [Fact]
    public void Validate_WithWeakPassword_ReportsPasswordWithTheReason()
    {
        var result = _validator.Validate(Valid() with { Password = "curta1234" });

        var error = Assert.Single(result.Errors);
        Assert.Equal(nameof(RegisterCommand.Password), error.PropertyName);
        Assert.Equal("A senha precisa ter pelo menos 10 caracteres.", error.ErrorMessage);
    }

    [Fact]
    public void Validate_WithoutTermsVersion_ReportsTerms()
    {
        var result = _validator.Validate(Valid() with { AcceptedTermsVersion = "" });

        Assert.Equal(nameof(RegisterCommand.AcceptedTermsVersion), Assert.Single(result.Errors).PropertyName);
    }

    private static RegisterCommand Valid() => new("Ana Lima", "ana@exemplo.com.br", "vidro azul na janela", "2026-10-28");
}
