using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class ProviderServiceConfiguration : IEntityTypeConfiguration<ProviderService>
{
    public void Configure(EntityTypeBuilder<ProviderService> builder)
    {
        builder.ToTable("provider_services");
        builder.HasKey(service => service.Id);
        builder.Property(service => service.Id).ValueGeneratedNever();
        builder.HasIndex(service => service.ProviderId);
        builder.Property(service => service.Title).HasMaxLength(ProviderService.TitleMaxLength);
        builder.Property(service => service.Description).HasMaxLength(ProviderService.DescriptionMaxLength);
        builder.Property(service => service.PriceUnit).HasConversion<string>().HasMaxLength(20);
    }
}
