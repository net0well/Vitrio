using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Auditing;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(audit => audit.Id);
        builder.Property(audit => audit.Id).ValueGeneratedNever();
        builder.Property(audit => audit.Action).HasMaxLength(64);
        builder.Property(audit => audit.TargetType).HasMaxLength(32);
        builder.Property(audit => audit.IpPrefix).HasMaxLength(64);
        builder.Property(audit => audit.TraceId).HasMaxLength(64);
        builder.HasIndex(audit => audit.OccurredAt);
        builder.HasIndex(audit => new { audit.ActorId, audit.OccurredAt });
        builder.HasIndex(audit => new { audit.TargetType, audit.TargetId });
    }
}
