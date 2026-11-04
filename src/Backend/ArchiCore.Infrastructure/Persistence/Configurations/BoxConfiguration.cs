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
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);

        builder.Property(x => x.BoxCode)
            .HasConversion(DomainValueConverters.BoxCodeConverter)
            .HasMaxLength(20)
            .IsRequired();
        builder.HasIndex(x => x.BoxCode).IsUnique();

        builder.Property(x => x.Barcode)
            .HasConversion(DomainValueConverters.BarcodeConverter)
            .HasMaxLength(100);
        builder.HasIndex(x => x.Barcode).IsUnique();

        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.WarehouseLocationId);

        builder.OwnsMany(x => x.FolderReferences, folderReference =>
        {
            folderReference.ToTable("BoxFolderReferences");
            folderReference.WithOwner().HasForeignKey("BoxId");
            folderReference.Property<Guid>("BoxId");
            folderReference.Property(x => x.FolderId).IsRequired();
            folderReference.HasKey("BoxId", nameof(BoxFolderReference.FolderId));
            folderReference.HasIndex(nameof(BoxFolderReference.FolderId));
        });

        builder.Navigation(x => x.FolderReferences).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Ignore(x => x.FolderIds);
    }
}
