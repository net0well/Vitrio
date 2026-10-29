using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class AdminGrantConfiguration : IEntityTypeConfiguration<AdminGrant>
{
    public void Configure(EntityTypeBuilder<AdminGrant> builder)
    {
        builder.ToTable("admin_grants");
        builder.HasKey(grant => grant.UserId);
        builder.HasOne<User>().WithOne().HasForeignKey<AdminGrant>(grant => grant.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
