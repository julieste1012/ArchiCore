using ArchiCore.Domain.Boxes;
using ArchiCore.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class BoxConfiguration : IEntityTypeConfiguration<Box>
{
    public void Configure(EntityTypeBuilder<Box> builder)
    {
        builder.ToTable("Boxes");
        builder.HasKey(box => box.Id);
        builder.Property(box => box.Id).ValueGeneratedNever();

        builder.Property(box => box.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(box => box.UpdatedAt)
            .HasColumnType("timestamp with time zone");
        builder.Ignore(box => box.DomainEvents);

        builder.Property(box => box.BoxCode)
            .HasConversion(DomainValueConverters.BoxCodeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(box => box.BoxCode)
            .IsUnique()
            .HasDatabaseName("UX_Boxes_BoxCode");

        builder.Property(box => box.Barcode)
            .HasConversion(DomainValueConverters.BarcodeConverter)
            .HasMaxLength(100);
        builder.HasIndex(box => box.Barcode)
            .IsUnique()
            .HasFilter("\"Barcode\" IS NOT NULL")
            .HasDatabaseName("UX_Boxes_Barcode");

        builder.Property(box => box.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
    }
}
