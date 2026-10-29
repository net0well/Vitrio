using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;
using Vitrio.Infrastructure.Persistence.Protection;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("addresses");
        builder.HasKey(address => address.Id);
        builder.Property(address => address.Id).ValueGeneratedNever();
        builder.Property(address => address.Label).HasMaxLength(Address.LabelMaxLength);
        builder.Property(address => address.Cep).IsPersonalData();
        builder.Property(address => address.Street).IsPersonalData();
        builder.Property(address => address.Number).IsPersonalData();
        builder.Property(address => address.Complement).IsPersonalData();
        builder.Property(address => address.Neighborhood).IsPersonalData();
        builder.Property(address => address.City).HasMaxLength(PostalLocation.CityMaxLength);
        builder.Property(address => address.State).HasMaxLength(2).IsFixedLength();
        builder.HasIndex(address => address.UserId);
        builder.HasOne<User>().WithMany().HasForeignKey(address => address.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
