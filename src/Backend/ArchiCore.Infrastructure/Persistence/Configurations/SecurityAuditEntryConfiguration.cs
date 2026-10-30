using ArchiCore.Infrastructure.Persistence.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class SecurityAuditEntryConfiguration : IEntityTypeConfiguration<SecurityAuditEntry>
{
    public void Configure(EntityTypeBuilder<SecurityAuditEntry> builder)
    {
        builder.ToTable("SecurityAuditEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Id).ValueGeneratedNever();

        builder.Property(entry => entry.OccurredOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(entry => entry.Action)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(entry => entry.Outcome)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(entry => entry.ActorRole).HasMaxLength(64);
        builder.Property(entry => entry.TargetRole).HasMaxLength(64);

        // IDs are intentionally not foreign keys: audit history must survive account lifecycle changes.
        builder.HasIndex(entry => entry.OccurredOnUtc)
            .HasDatabaseName("IX_SecurityAuditEntries_OccurredOnUtc");
        builder.HasIndex(entry => entry.Action)
            .HasDatabaseName("IX_SecurityAuditEntries_Action");
        builder.HasIndex(entry => entry.ActorUserId)
            .HasDatabaseName("IX_SecurityAuditEntries_ActorUserId");
        builder.HasIndex(entry => entry.TargetUserId)
            .HasDatabaseName("IX_SecurityAuditEntries_TargetUserId");
    }
}
