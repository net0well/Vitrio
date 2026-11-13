using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vitrio.Domain.Accounts;
using Vitrio.Domain.Common;
using Vitrio.Domain.Orders;
using Vitrio.Domain.Providers;
using Vitrio.Infrastructure.Persistence.Protection;

namespace Vitrio.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    private const int EnumMaxLength = 30;

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(order => order.Id);
        builder.Property(order => order.Id).ValueGeneratedNever();
        builder.Property<uint>("xmin").HasColumnType("xid").IsRowVersion();
        builder.Ignore(order => order.DomainEvents);

        builder.Property(order => order.ServiceTitle).HasMaxLength(80);
        builder.Property(order => order.Title).HasMaxLength(80);
        builder.Property(order => order.Description).HasMaxLength(500);
        builder.Property(order => order.CustomerDisplayName).HasMaxLength(120);
        builder.Property(order => order.PreferredPeriod).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(order => order.Status).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(order => order.DeclineReason).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(order => order.DeclineDetails).HasMaxLength(Order.MaxReasonDetailsLength);
        builder.Property(order => order.CompletedBy).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(order => order.CancelledBy).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(order => order.CancellationReason).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(order => order.CancellationDetails).HasMaxLength(Order.MaxReasonDetailsLength);

        builder.OwnsOne(order => order.Place, place =>
        {
            place.Property(value => value.CityIbgeCode).HasColumnName("city_ibge_code");
            place.Property(value => value.CityName).HasColumnName("city_name").HasMaxLength(120);
            place.Property(value => value.State).HasColumnName("state").HasMaxLength(2);
            place.Property(value => value.Neighborhood).HasColumnName("neighborhood").HasMaxLength(120);
            place.Property(value => value.TimeZoneId).HasColumnName("time_zone").HasMaxLength(64);
            place.Property(value => value.Latitude).HasColumnName("customer_latitude");
            place.Property(value => value.Longitude).HasColumnName("customer_longitude");
            place.Property(value => value.IsApproximate).HasColumnName("customer_point_is_approximate");
        });
        builder.Navigation(order => order.Place).IsRequired();

        builder.OwnsOne(order => order.Address, address =>
        {
            address.Property(value => value.Street).HasColumnName("address_street").IsPersonalData();
            address.Property(value => value.Number).HasColumnName("address_number").IsPersonalData();
            address.Property(value => value.Complement).HasColumnName("address_complement").IsPersonalData();
            address.Property(value => value.Cep).HasColumnName("address_cep").IsPersonalData();
        });

        builder.OwnsOne(order => order.Contact, contact =>
        {
            contact.Property(value => value.FullName).HasColumnName("customer_full_name").IsPersonalData();
            contact.Property(value => value.Phone).HasColumnName("customer_phone").IsPersonalData();
        });

        builder.OwnsOne(order => order.Quote, quote =>
        {
            quote.Property(value => value.Type).HasColumnName("quote_type").HasConversion<string>().HasMaxLength(EnumMaxLength);
            quote.Property(value => value.Amount)
                .HasColumnName("quote_amount")
                .HasColumnType("numeric(12,2)")
                .HasConversion(money => money.Reais, reais => new Money((long)(reais * 100)));
            quote.Property(value => value.MaterialIncluded).HasColumnName("quote_material_included");
            quote.Property(value => value.MaterialDetails).HasColumnName("quote_material_details").HasMaxLength(Quote.MaxMaterialDetailsLength);
            quote.Property(value => value.Message).HasColumnName("quote_message").HasMaxLength(Quote.MaxMessageLength);
            quote.Property(value => value.SentAt).HasColumnName("quote_sent_at");
            quote.Property(value => value.Version).HasColumnName("quote_version");
            quote.OwnsOne(value => value.Schedule, schedule => MapSchedule(schedule, "quote"));
            quote.Navigation(value => value.Schedule).IsRequired();
        });

        builder.OwnsOne(order => order.Schedule, schedule =>
        {
            MapSchedule(schedule, "scheduled");
            schedule.HasIndex(value => value.StartsAt);
        });

        builder.HasMany(order => order.Photos).WithOne().HasForeignKey(photo => photo.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(order => order.CustomerId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Provider>().WithMany().HasForeignKey(order => order.ProviderId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(order => new { order.CustomerId, order.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(order => new { order.ProviderId, order.Status });
        builder.HasIndex(order => new { order.Status, order.RespondBy });
        builder.HasIndex(order => new { order.Status, order.QuoteValidUntil });
    }

    private static void MapSchedule<TOwner>(OwnedNavigationBuilder<TOwner, ServiceSchedule> schedule, string prefix)
        where TOwner : class
    {
        schedule.Property(value => value.Date).HasColumnName($"{prefix}_date");
        schedule.Property(value => value.Start).HasColumnName($"{prefix}_start_time");
        schedule.Property(value => value.DurationMinutes).HasColumnName($"{prefix}_duration_minutes");
        schedule.Property(value => value.StartsAt).HasColumnName($"{prefix}_start");
        schedule.Property(value => value.EndsAt).HasColumnName($"{prefix}_end");
        schedule.Ignore(value => value.End);
    }
}
