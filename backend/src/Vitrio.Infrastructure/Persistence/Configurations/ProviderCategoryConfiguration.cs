using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Categories;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class ProviderCategoryConfiguration : IEntityTypeConfiguration<ProviderCategory>
{
    public void Configure(EntityTypeBuilder<ProviderCategory> builder)
    {
        builder.ToTable("provider_categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();
        builder.HasIndex(category => new { category.ProviderId, category.CategoryId }).IsUnique();
        builder.HasIndex(category => category.CategoryId);
        builder.HasOne<Category>().WithMany().HasForeignKey(category => category.CategoryId).OnDelete(DeleteBehavior.Restrict);
    }
}
