using ArchiCore.Domain.Folders;
using ArchiCore.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.ToTable("Folders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.FolderCode).HasConversion(DomainValueConverters.FolderCodeConverter).HasMaxLength(20).IsRequired();
        builder.HasIndex(x => x.FolderCode).IsUnique();
        builder.Property(x => x.ArchiveId).IsRequired();
        builder.Property(x => x.ExpedientId).IsRequired();
        builder.Property(x => x.BoxId);
        builder.Property(x => x.LoanId);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.ArchiveId);
        builder.HasIndex(x => x.ExpedientId);
        builder.HasIndex(x => x.BoxId);
        builder.HasIndex(x => x.LoanId);

        builder.OwnsMany(x => x.Documents, document =>
        {
            document.ToTable("FolderDocuments");
            document.WithOwner().HasForeignKey("FolderId");
            document.Property<Guid>("FolderId");
            document.HasKey("FolderId", nameof(FolderDocument.Id));
            document.Property(x => x.Id).ValueGeneratedNever();
            document.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            document.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            document.Ignore(x => x.DomainEvents);
            document.Property(x => x.Title).HasMaxLength(300).IsRequired();
            document.Property(x => x.DocumentDate).HasColumnType("timestamp with time zone");

            document.OwnsMany(x => x.Folios, folio =>
            {
                folio.ToTable("DocumentFolios");
                folio.WithOwner().HasForeignKey("FolderId", "DocumentId")
                    .HasPrincipalKey("FolderId", nameof(FolderDocument.Id));
                folio.Property<Guid>("FolderId");
                folio.Property<Guid>("DocumentId");
                folio.HasKey("FolderId", "DocumentId", nameof(Folio.Id));
                folio.Property(x => x.Id).ValueGeneratedNever();
                folio.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
                folio.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
                folio.Ignore(x => x.DomainEvents);
                folio.Property(x => x.Number).IsRequired();
                folio.HasIndex("FolderId", "DocumentId", nameof(Folio.Number)).IsUnique();
            });

            document.OwnsMany(x => x.Annexes, annex =>
            {
                annex.ToTable("DocumentAnnexes");
                annex.WithOwner().HasForeignKey("FolderId", "DocumentId")
                    .HasPrincipalKey("FolderId", nameof(FolderDocument.Id));
                annex.Property<Guid>("FolderId");
                annex.Property<Guid>("DocumentId");
                annex.HasKey("FolderId", "DocumentId", nameof(Annex.Id));
                annex.Property(x => x.Id).ValueGeneratedNever();
                annex.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
                annex.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
                annex.Ignore(x => x.DomainEvents);
                annex.Property(x => x.Title).HasMaxLength(250).IsRequired();
                annex.Property(x => x.Sequence).IsRequired();
                annex.HasIndex("FolderId", "DocumentId", nameof(Annex.Sequence)).IsUnique();
            });
            document.Navigation(x => x.Folios).UsePropertyAccessMode(PropertyAccessMode.Field);
            document.Navigation(x => x.Annexes).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
