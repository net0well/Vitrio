using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> builder)
    {
        builder.ToTable("devices");
        builder.HasKey(device => device.InstallationId);
        builder.Property(device => device.InstallationId).HasMaxLength(Device.InstallationIdMaxLength);
        builder.Property(device => device.Platform).HasConversion<string>().HasMaxLength(10);
        builder.Property(device => device.PushToken).HasMaxLength(Device.PushTokenMaxLength);
        builder.HasIndex(device => device.UserId);
        builder.HasOne<User>().WithMany().HasForeignKey(device => device.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
