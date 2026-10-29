using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Infrastructure.Persistence.Cities;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class CityRecordConfiguration : IEntityTypeConfiguration<CityRecord>
{
    public void Configure(EntityTypeBuilder<CityRecord> builder)
    {
        builder.ToTable("cities");
        builder.HasKey(city => city.IbgeCode);
        builder.Property(city => city.IbgeCode).ValueGeneratedNever();
        builder.Property(city => city.Name).HasMaxLength(80);
        builder.Property(city => city.NameNormalized).HasMaxLength(80);
        builder.Property(city => city.State).HasMaxLength(2).IsFixedLength();
        builder.Property(city => city.TimeZone).HasMaxLength(40);
        builder.Property(city => city.Boundary).HasColumnType("geography (multipolygon, 4326)");
        builder.Property(city => city.Centroid).HasColumnType("geography (point, 4326)");
        builder.HasIndex(city => city.NameNormalized).HasMethod("gin").HasOperators("gin_trgm_ops");
        builder.HasIndex(city => city.State);
        builder.HasIndex(city => city.Boundary).HasMethod("gist");
        builder.HasIndex(city => city.Centroid).HasMethod("gist");
    }
}
