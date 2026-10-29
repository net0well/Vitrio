namespace Vitrio.Infrastructure.Persistence.Protection;

internal interface IPersonalDataProtector
{
    string Protect(string value);

    string Unprotect(string protectedValue);
}
