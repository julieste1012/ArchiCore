using ArchiCore.Domain.Archives;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class ArchiveConfiguration : IEntityTypeConfiguration<Archive>
{
    public void Configure(EntityTypeBuilder<Archive> builder)
    {
        builder.ToTable("Archives");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.HasIndex(x => x.ProjectId).IsUnique();

        builder.OwnsMany(x => x.Sections, section =>
        {
            section.ToTable("ArchiveSections");
            section.WithOwner().HasForeignKey("ArchiveId");
            section.Property<Guid>("ArchiveId");
            section.HasKey("ArchiveId", nameof(ArchiveSection.Id));
            section.Property(x => x.Id).ValueGeneratedNever();
            section.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
            section.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            section.Ignore(x => x.DomainEvents);
            section.Property(x => x.Code).HasMaxLength(50).IsRequired();
            section.Property(x => x.Name).HasMaxLength(250).IsRequired();
            section.HasIndex("ArchiveId", nameof(ArchiveSection.Code)).IsUnique();

            section.OwnsMany(x => x.Series, series =>
            {
                series.ToTable("ArchiveSeries");
                series.WithOwner().HasForeignKey("ArchiveId", "SectionId")
                    .HasPrincipalKey("ArchiveId", nameof(ArchiveSection.Id));
                series.Property<Guid>("ArchiveId");
                series.Property<Guid>("SectionId");
                series.HasKey("ArchiveId", "SectionId", nameof(ArchiveSeries.Id));
                series.Property(x => x.Id).ValueGeneratedNever();
                series.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
                series.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
                series.Ignore(x => x.DomainEvents);
                series.Property(x => x.Code).HasMaxLength(50).IsRequired();
                series.Property(x => x.Name).HasMaxLength(250).IsRequired();
                series.HasIndex("ArchiveId", "SectionId", nameof(ArchiveSeries.Code)).IsUnique();

                series.OwnsMany(x => x.Subseries, subseries =>
                {
                    subseries.ToTable("ArchiveSubseries");
                    subseries.WithOwner().HasForeignKey("ArchiveId", "SectionId", "SeriesId")
                        .HasPrincipalKey("ArchiveId", "SectionId", nameof(ArchiveSeries.Id));
                    subseries.Property<Guid>("ArchiveId");
                    subseries.Property<Guid>("SectionId");
                    subseries.Property<Guid>("SeriesId");
                    subseries.HasKey("ArchiveId", "SectionId", "SeriesId", nameof(ArchiveSubseries.Id));
                    subseries.Property(x => x.Id).ValueGeneratedNever();
                    subseries.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
                    subseries.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
                    subseries.Ignore(x => x.DomainEvents);
                    subseries.Property(x => x.Code).HasMaxLength(50).IsRequired();
                    subseries.Property(x => x.Name).HasMaxLength(250).IsRequired();
                    subseries.HasIndex("ArchiveId", "SectionId", "SeriesId", nameof(ArchiveSubseries.Code)).IsUnique();

                    subseries.OwnsMany(x => x.Expedients, expedient =>
                    {
                        expedient.ToTable("ArchiveExpedients");
                        expedient.WithOwner().HasForeignKey("ArchiveId", "SectionId", "SeriesId", "SubseriesId")
                            .HasPrincipalKey("ArchiveId", "SectionId", "SeriesId", nameof(ArchiveSubseries.Id));
                        expedient.Property<Guid>("ArchiveId");
                        expedient.Property<Guid>("SectionId");
                        expedient.Property<Guid>("SeriesId");
                        expedient.Property<Guid>("SubseriesId");
                        expedient.HasKey("ArchiveId", "SectionId", "SeriesId", "SubseriesId", nameof(Expedient.Id));
                        expedient.Property(x => x.Id).ValueGeneratedNever();
                        expedient.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
                        expedient.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
                        expedient.Ignore(x => x.DomainEvents);
                        expedient.Property(x => x.Reference).HasMaxLength(100).IsRequired();
                        expedient.Property(x => x.Title).HasMaxLength(300).IsRequired();
                        expedient.HasIndex("ArchiveId", "SectionId", "SeriesId", "SubseriesId", nameof(Expedient.Reference)).IsUnique();
                    });
                    subseries.Navigation(x => x.Expedients).UsePropertyAccessMode(PropertyAccessMode.Field);
                });
                series.Navigation(x => x.Subseries).UsePropertyAccessMode(PropertyAccessMode.Field);
            });
            section.Navigation(x => x.Series).UsePropertyAccessMode(PropertyAccessMode.Field);
        });
        builder.Navigation(x => x.Sections).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
