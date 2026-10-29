using FluentValidation;
using Vitrio.Domain.Accounts;

namespace Vitrio.Application.Accounts.Common;

internal static class AccountRules
{
    public static IRuleBuilderOptionsConditions<T, string> MustBeEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.Custom((value, context) =>
        {
            var email = EmailAddress.Create(value);
            if (email.IsFailure)
            {
                context.AddFailure(email.Error.Message);
            }
        });

    public static IRuleBuilderOptionsConditions<T, string> MustBeDisplayName<T>(this IRuleBuilder<T, string> rule) =>
        rule.Custom((value, context) =>
        {
            var displayName = DisplayName.Create(value);
            if (displayName.IsFailure)
            {
                context.AddFailure(displayName.Error.Message);
            }
        });

    public static IRuleBuilderOptionsConditions<T, string> MustBeStrongPassword<T>(
        this IRuleBuilder<T, string> rule,
        Func<T, string?> email) =>
        rule.Custom((value, context) =>
        {
            var problem = PasswordPolicy.FindProblem(value, email(context.InstanceToValidate));
            if (problem is not null)
            {
                context.AddFailure(MessageFor(problem.Value));
            }
        });

    public static IRuleBuilderOptions<T, string> MustBeCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.Matches("^[0-9]{6}$").WithMessage("Digite os 6 números do código.");

    public static string MessageFor(PasswordProblem problem) => problem switch
    {
        PasswordProblem.TooShort => $"A senha precisa ter pelo menos {PasswordPolicy.MinLength} caracteres.",
        PasswordProblem.TooLong => $"A senha pode ter no máximo {PasswordPolicy.MaxLength} caracteres.",
        PasswordProblem.Common => "Essa senha é muito comum. Escolha outra.",
        PasswordProblem.SameAsEmail => "A senha não pode ser igual ao e-mail.",
        _ => throw new ArgumentOutOfRangeException(nameof(problem), problem, null)
    };
}
