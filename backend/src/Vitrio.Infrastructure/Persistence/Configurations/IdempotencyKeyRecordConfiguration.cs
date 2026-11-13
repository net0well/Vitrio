using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;
using Vitrio.Infrastructure.Persistence.Idempotency;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class IdempotencyKeyRecordConfiguration : IEntityTypeConfiguration<IdempotencyKeyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyKeyRecord> builder)
    {
        builder.ToTable("idempotency_keys");
        builder.HasKey(record => new { record.UserId, record.Key });
        builder.Property(record => record.RequestHash).HasMaxLength(64);
        builder.Property(record => record.ResponseBody).HasColumnType("jsonb");
        builder.HasIndex(record => record.CreatedAt);
        builder.HasOne<User>().WithMany().HasForeignKey(record => record.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
