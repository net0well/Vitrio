using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class OutboxDeliveryConfiguration : IEntityTypeConfiguration<OutboxDelivery>
{
    public void Configure(EntityTypeBuilder<OutboxDelivery> builder)
    {
        builder.ToTable("outbox_deliveries");
        builder.HasKey(delivery => new { delivery.MessageId, delivery.Handler });
        builder.Property(delivery => delivery.Handler).HasMaxLength(100);
        builder.Property(delivery => delivery.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(delivery => delivery.LastError).HasMaxLength(OutboxDelivery.LastErrorMaxLength);
        builder.HasOne<OutboxMessage>().WithMany().HasForeignKey(delivery => delivery.MessageId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(delivery => delivery.NextAttemptAt)
            .HasDatabaseName("ix_outbox_deliveries_pending")
            .HasFilter("status = 'Pending'");
    }
}
