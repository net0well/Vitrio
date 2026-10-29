using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.Id).ValueGeneratedNever();
        builder.Property(token => token.TokenHash).HasMaxLength(64);
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => token.FamilyId);
        builder.HasIndex(token => token.ExpiresAt);
        builder.HasOne<Session>().WithMany().HasForeignKey(token => token.SessionId).OnDelete(DeleteBehavior.Cascade);
    }
}
