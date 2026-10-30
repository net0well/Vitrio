using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence.Protection;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class MeiVerificationConfiguration : IEntityTypeConfiguration<MeiVerification>
{
    public void Configure(EntityTypeBuilder<MeiVerification> builder)
    {
        builder.ToTable("mei_verifications");
        builder.HasKey(mei => mei.ProviderId);
        builder.Property(mei => mei.Cnpj).HasMaxLength(Cnpj.Length).IsFixedLength();
        builder.HasIndex(mei => mei.Cnpj).IsUnique();
        builder.Property(mei => mei.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(mei => new { mei.Status, mei.CheckedAt });
        builder.Property(mei => mei.CompanyName).IsPersonalData();
        builder.Property(mei => mei.ReasonCode).HasMaxLength(40);
    }
}
