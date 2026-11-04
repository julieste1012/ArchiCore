using ArchiCore.Domain.Warehouses;
using ArchiCore.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.WarehouseCode)
            .HasConversion(DomainValueConverters.WarehouseCodeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(x => x.WarehouseCode).IsUnique();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.OwnsMany(x => x.Locations, location =>
        {
            location.ToTable("WarehouseLocations");
            location.WithOwner().HasForeignKey("WarehouseId");
            location.Property<Guid>("WarehouseId");
            location.HasKey("WarehouseId", nameof(WarehouseLocation.Id));
            location.Property(x => x.Id).ValueGeneratedNever();
            location.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            location.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            location.Ignore(x => x.DomainEvents);
            location.Property(x => x.Code).HasMaxLength(50).IsRequired();
            location.Property(x => x.Type).HasConversion<string>().HasMaxLength(24).IsRequired();
            location.Property(x => x.ParentLocationId);
            location.Property(x => x.Capacity);
            location.HasIndex("WarehouseId", nameof(WarehouseLocation.Code)).IsUnique();

            location.OwnsMany(x => x.BoxReferences, boxReference =>
            {
                boxReference.ToTable("WarehouseLocationBoxReferences");
                boxReference.WithOwner()
                    .HasForeignKey("WarehouseId", "LocationId")
                    .HasPrincipalKey("WarehouseId", nameof(WarehouseLocation.Id));
                boxReference.Property<Guid>("WarehouseId");
                boxReference.Property<Guid>("LocationId");
                boxReference.Property(x => x.BoxId).IsRequired();
                boxReference.HasKey("WarehouseId", "LocationId", nameof(WarehouseLocationBoxReference.BoxId));
                boxReference.HasIndex(nameof(WarehouseLocationBoxReference.BoxId));
            });

            location.Navigation(x => x.BoxReferences).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(x => x.Locations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
