using ArchiCore.Domain.Digitization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class DigitizationRequestConfiguration : IEntityTypeConfiguration<DigitizationRequest>
{
    public void Configure(EntityTypeBuilder<DigitizationRequest> builder)
    {
        builder.ToTable("DigitizationRequests");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.FolderId).IsRequired();
        builder.Property(x => x.RequestedByUserId).IsRequired();
        builder.Property(x => x.RequestedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.StartedByUserId);
        builder.Property(x => x.StartedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.CompletedByUserId);
        builder.Property(x => x.CompletedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.FolderId);
        builder.HasIndex(x => x.Status);

        builder.OwnsMany(x => x.Files, file =>
        {
            file.ToTable("DigitizedFiles");
            file.WithOwner().HasForeignKey("DigitizationRequestId");
            file.Property<Guid>("DigitizationRequestId");
            file.HasKey("DigitizationRequestId", nameof(DigitizedFile.Id));
            file.Property(x => x.Id).ValueGeneratedNever();
            file.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            file.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            file.Ignore(x => x.DomainEvents);
            file.Property(x => x.StorageObjectKey).HasMaxLength(512).IsRequired();
            file.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            file.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
            file.Property(x => x.SizeBytes).IsRequired();
            file.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
            file.Property(x => x.PageCount).IsRequired();
            file.HasIndex("DigitizationRequestId", nameof(DigitizedFile.StorageObjectKey)).IsUnique();
        });
        builder.Navigation(x => x.Files).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
