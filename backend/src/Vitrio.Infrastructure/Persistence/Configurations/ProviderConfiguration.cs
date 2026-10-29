using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence.Cities;
using Vitrio.Infrastructure.Persistence.Protection;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public const int ObjectKeyMaxLength = 200;

    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");
        builder.HasKey(provider => provider.Id);
        builder.Property(provider => provider.Id).ValueGeneratedNever();
        builder.HasIndex(provider => provider.UserId).IsUnique().HasFilter("status <> 'Removed'");
        builder.HasOne<User>().WithMany().HasForeignKey(provider => provider.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(provider => provider.DisplayName).HasMaxLength(DisplayName.MaxLength);
        builder.Property(provider => provider.Headline).HasMaxLength(Provider.HeadlineMaxLength);
        builder.Property(provider => provider.About).HasMaxLength(Provider.AboutMaxLength);
        builder.Property(provider => provider.AvatarKey).HasMaxLength(ObjectKeyMaxLength);
        builder.Property(provider => provider.ContactPhone).IsPersonalData();
        builder.Property(provider => provider.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasOne<CityRecord>().WithMany().HasForeignKey(provider => provider.CityIbgeCode).OnDelete(DeleteBehavior.Restrict);
        builder.Property(provider => provider.BaseCep).IsPersonalData();
        builder.Property(provider => provider.BaseStreet).IsPersonalData();
        builder.Property(provider => provider.BaseNumber).IsPersonalData();
        builder.Property(provider => provider.BaseComplement).IsPersonalData();
        builder.Property(provider => provider.BaseNeighborhood).IsPersonalData();
        builder.Property(provider => provider.BaseLocation).IsPersonalData();
        builder.Property(provider => provider.RatingScore).HasPrecision(3, 2);
        builder.Property(provider => provider.ResponseTimeBucket).HasMaxLength(20);
        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        builder.HasMany(provider => provider.Categories).WithOne().HasForeignKey(category => category.ProviderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(provider => provider.Services).WithOne().HasForeignKey(service => service.ProviderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(provider => provider.Neighborhoods).WithOne().HasForeignKey(neighborhood => neighborhood.ProviderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(provider => provider.Availability).WithOne().HasForeignKey(slot => slot.ProviderId).OnDelete(DeleteBehavior.Cascade);
        builder.Ignore(provider => provider.HasServiceArea);
        builder.Ignore(provider => provider.MissingForPublication);
        builder.Ignore(provider => provider.DomainEvents);
    }
}
