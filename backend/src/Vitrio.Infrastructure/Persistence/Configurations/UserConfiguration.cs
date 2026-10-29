using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;
using Vitrio.Infrastructure.Persistence.Protection;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public const int PasswordHashMaxLength = 256;

    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();
        builder.Property(user => user.Email).HasColumnType("citext").HasMaxLength(EmailAddress.MaxLength);
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.DisplayName).HasMaxLength(DisplayName.MaxLength);
        builder.Property(user => user.PasswordHash).HasMaxLength(PasswordHashMaxLength);
        builder.Property(user => user.PhoneNumber).IsPersonalData();
        builder.Property(user => user.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(user => user.AcceptedTermsVersion).HasMaxLength(User.TermsVersionMaxLength);
        builder.HasIndex(user => user.CreatedAt)
            .HasDatabaseName("ix_users_pending_verification")
            .HasFilter("status = 'PendingVerification'");
        builder.Ignore(user => user.DomainEvents);
    }
}
