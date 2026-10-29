using ArchiCore.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class DomainEventAuditEntryConfiguration : IEntityTypeConfiguration<DomainEventAuditEntry>
{
    public void Configure(EntityTypeBuilder<DomainEventAuditEntry> builder)
    {
        builder.ToTable("DomainEventAuditEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.EventType).HasMaxLength(512).IsRequired();
        builder.Property(x => x.OccurredOnUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.RecordedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.Payload).HasColumnType("jsonb").IsRequired();
        builder.HasIndex(x => x.OccurredOnUtc).HasDatabaseName("IX_DomainEventAuditEntries_OccurredOnUtc");
        builder.HasIndex(x => x.EventType).HasDatabaseName("IX_DomainEventAuditEntries_EventType");
    }
}
