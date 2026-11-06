using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Orders;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class OrderPhotoConfiguration : IEntityTypeConfiguration<OrderPhoto>
{
    public void Configure(EntityTypeBuilder<OrderPhoto> builder)
    {
        builder.ToTable("order_photos");
        builder.HasKey(photo => photo.Id);
        builder.Property(photo => photo.Id).ValueGeneratedNever();
        builder.Property(photo => photo.UploadKey).HasMaxLength(ProviderConfiguration.ObjectKeyMaxLength);
        builder.Property(photo => photo.ObjectKey).HasMaxLength(ProviderConfiguration.ObjectKeyMaxLength);
        builder.Property(photo => photo.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(photo => photo.OrderId);
        builder.HasIndex(photo => photo.CreatedAt).HasDatabaseName("ix_order_photos_processing").HasFilter("status = 'Processing'");
    }
}
