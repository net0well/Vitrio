using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("availability_slots");
        builder.HasKey(slot => slot.Id);
        builder.Property(slot => slot.Id).ValueGeneratedNever();
        builder.HasIndex(slot => slot.ProviderId);
    }
}
