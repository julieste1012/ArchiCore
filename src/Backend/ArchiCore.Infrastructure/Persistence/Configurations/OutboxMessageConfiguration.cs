using ArchiCore.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.EventType)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(x => x.OccurredOnUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.EnqueuedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.Payload)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.ProcessedAtUtc)
            .HasColumnType("timestamp with time zone");

        // Index the pending queue without including already processed records.
        builder.HasIndex(x => x.EnqueuedAtUtc)
            .HasDatabaseName("IX_OutboxMessages_Pending")
            .HasFilter("\\"ProcessedAtUtc\\" IS NULL");
    }
}
