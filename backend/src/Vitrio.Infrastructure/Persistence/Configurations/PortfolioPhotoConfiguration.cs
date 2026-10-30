using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class PortfolioPhotoConfiguration : IEntityTypeConfiguration<PortfolioPhoto>
{
    public void Configure(EntityTypeBuilder<PortfolioPhoto> builder)
    {
        builder.ToTable("portfolio_photos");
        builder.HasKey(photo => photo.Id);
        builder.Property(photo => photo.Id).ValueGeneratedNever();
        builder.HasIndex(photo => new { photo.ProviderId, photo.SortOrder });
        builder.HasIndex(photo => photo.CreatedAt).HasDatabaseName("ix_portfolio_photos_processing").HasFilter("status = 'Processing'");
        builder.Property(photo => photo.UploadKey).HasMaxLength(ProviderConfiguration.ObjectKeyMaxLength);
        builder.Property(photo => photo.ObjectKey).HasMaxLength(ProviderConfiguration.ObjectKeyMaxLength);
        builder.Property(photo => photo.ThumbnailKey).HasMaxLength(ProviderConfiguration.ObjectKeyMaxLength);
        builder.Property(photo => photo.Caption).HasMaxLength(PortfolioPhoto.CaptionMaxLength);
        builder.Property(photo => photo.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(photo => photo.RejectionReason).HasMaxLength(40);
    }
}
