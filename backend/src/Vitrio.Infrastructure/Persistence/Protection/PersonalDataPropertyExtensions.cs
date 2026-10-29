using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Vitrio.Infrastructure.Persistence.Protection;

internal static class PersonalDataPropertyExtensions
{
    public const string Annotation = "Vitrio:PersonalData";

    public static PropertyBuilder<TProperty> IsPersonalData<TProperty>(this PropertyBuilder<TProperty> property) =>
        property.HasColumnType("text").HasAnnotation(Annotation, true);
}
