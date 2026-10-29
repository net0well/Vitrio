using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class ServedNeighborhoodConfiguration : IEntityTypeConfiguration<ServedNeighborhood>
{
    public void Configure(EntityTypeBuilder<ServedNeighborhood> builder)
    {
        builder.ToTable("served_neighborhoods");
        builder.HasKey(neighborhood => neighborhood.Id);
        builder.Property(neighborhood => neighborhood.Id).ValueGeneratedNever();
        builder.HasIndex(neighborhood => new { neighborhood.ProviderId, neighborhood.NameNormalized }).IsUnique();
        builder.Property(neighborhood => neighborhood.Name).HasMaxLength(ServedNeighborhood.MaxLength);
        builder.Property(neighborhood => neighborhood.NameNormalized).HasMaxLength(ServedNeighborhood.MaxLength);
    }
}
