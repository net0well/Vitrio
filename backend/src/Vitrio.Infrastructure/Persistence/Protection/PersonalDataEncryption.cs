using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Vitrio.Infrastructure.Persistence.Protection;

internal static class PersonalDataEncryption
{
    public static void Apply(ModelBuilder modelBuilder, IDbContextOptions options)
    {
        var applicationServices = options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
        if (applicationServices is null)
        {
            return;
        }

        var protector = applicationServices.GetService<IPersonalDataProtector>()
            ?? throw new InvalidOperationException("Personal data must be encrypted, but no protector is registered.");
        var converter = new PersonalDataConverter(protector);
        foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(entity => entity.GetProperties()))
        {
            if (property.FindAnnotation(PersonalDataPropertyExtensions.Annotation) is not null)
            {
                property.SetValueConverter(converter);
            }
        }
    }
}
