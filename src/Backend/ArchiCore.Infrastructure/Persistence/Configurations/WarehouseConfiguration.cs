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
    }
}
