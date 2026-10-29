using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Vitrio.Infrastructure.Persistence.Protection;

internal sealed class PersonalDataConverter(IPersonalDataProtector protector)
    : ValueConverter<string, string>(value => protector.Protect(value), stored => protector.Unprotect(stored));
