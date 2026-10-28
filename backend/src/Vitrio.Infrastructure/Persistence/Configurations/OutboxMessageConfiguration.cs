using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Infrastructure.Persistence.Outbox;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Id).ValueGeneratedNever();
        builder.Property(message => message.Type).HasMaxLength(300);
        builder.Property(message => message.Content).HasColumnType("jsonb");
        builder.Property(message => message.LastError).HasMaxLength(2000);
        builder.HasIndex(message => message.OccurredAt).HasFilter("processed_at IS NULL");
    }
}
