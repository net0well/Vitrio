using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class EmailCodeConfiguration : IEntityTypeConfiguration<EmailCode>
{
    public void Configure(EntityTypeBuilder<EmailCode> builder)
    {
        builder.ToTable("email_codes");
        builder.HasKey(code => code.Id);
        builder.Property(code => code.Id).ValueGeneratedNever();
        builder.Property(code => code.Email).HasMaxLength(EmailAddress.MaxLength);
        builder.Property(code => code.Purpose).HasConversion<string>().HasMaxLength(30);
        builder.Property(code => code.CodeHash).HasMaxLength(64);
        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        builder.HasIndex(code => new { code.Email, code.CreatedAt });
        builder.HasIndex(code => code.ExpiresAt);
    }
}
