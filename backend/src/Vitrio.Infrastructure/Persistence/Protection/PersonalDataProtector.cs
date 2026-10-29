using Microsoft.AspNetCore.DataProtection;

namespace Vitrio.Infrastructure.Persistence.Protection;

internal sealed class PersonalDataProtector(IDataProtectionProvider provider) : IPersonalDataProtector
{
    private const string Purpose = "Vitrio.PersonalData.v1";

    private readonly IDataProtector _protector = provider.CreateProtector(Purpose);

    public string Protect(string value) => _protector.Protect(value);

    public string Unprotect(string protectedValue) => _protector.Unprotect(protectedValue);
}
