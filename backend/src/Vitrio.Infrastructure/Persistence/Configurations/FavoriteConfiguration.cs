using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Favorites;
using Vitrio.Domain.Providers;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("favorites");
        builder.HasKey(favorite => favorite.Id);
        builder.Property(favorite => favorite.Id).ValueGeneratedNever();
        builder.HasIndex(favorite => new { favorite.UserId, favorite.ProviderId }).IsUnique();
        builder.HasIndex(favorite => new { favorite.UserId, favorite.CreatedAt }).IsDescending(false, true);
        builder.HasOne<User>().WithMany().HasForeignKey(favorite => favorite.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<Provider>().WithMany().HasForeignKey(favorite => favorite.ProviderId).OnDelete(DeleteBehavior.Cascade);
    }
}
