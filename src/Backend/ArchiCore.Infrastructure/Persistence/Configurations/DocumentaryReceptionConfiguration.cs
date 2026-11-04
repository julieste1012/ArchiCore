using ArchiCore.Domain.Contracts;
using ArchiCore.Domain.Customers;
using ArchiCore.Domain.DocumentaryReception;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class DocumentaryReceptionConfiguration : IEntityTypeConfiguration<DocumentaryReception>
{
    public void Configure(EntityTypeBuilder<DocumentaryReception> builder)
    {
        builder.ToTable("DocumentaryReceptions");
        builder.HasKey(reception => reception.Id);
        builder.Property(reception => reception.Id).ValueGeneratedNever();

        builder.Property(reception => reception.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(reception => reception.UpdatedAt)
            .HasColumnType("timestamp with time zone");
        builder.Ignore(reception => reception.DomainEvents);

        builder.Property(reception => reception.ContractId).IsRequired();
        builder.Property(reception => reception.CustomerId).IsRequired();
        builder.HasOne<Contract>()
            .WithMany()
            .HasForeignKey(reception => reception.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(reception => reception.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(reception => reception.ReceivedByUserId).IsRequired();
        builder.Property(reception => reception.ReceivedAtUtc)
            .HasColumnType("timestamp with time zone")
            .IsRequired();
        builder.Property(reception => reception.InitialObservations).HasMaxLength(2000);
        builder.Property(reception => reception.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        builder.Property(reception => reception.InspectedByUserId);
        builder.Property(reception => reception.InspectionCompletedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(reception => reception.SafeToAccept);
        builder.Property(reception => reception.InspectionObservations).HasMaxLength(2000);

        builder.Property(reception => reception.DeclaredBoxCount);
        builder.Property(reception => reception.DeclaredFolderCount);
        builder.Property(reception => reception.CountedBoxCount);
        builder.Property(reception => reception.CountedFolderCount);
        builder.Property(reception => reception.DiscrepancyReport).HasMaxLength(4000);
        builder.Property(reception => reception.CountedByUserId);
        builder.Property(reception => reception.CountedAtUtc).HasColumnType("timestamp with time zone");

        builder.Property(reception => reception.ApprovedByUserId);
        builder.Property(reception => reception.ApprovedAtUtc).HasColumnType("timestamp with time zone");

        builder.Property(reception => reception.SuspendedByUserId);
        builder.Property(reception => reception.SuspendedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(reception => reception.SuspensionReason).HasMaxLength(1000);

        builder.HasIndex(reception => new { reception.ContractId, reception.ReceivedAtUtc })
            .HasDatabaseName("IX_DocumentaryReceptions_ContractId_ReceivedAtUtc");
        builder.HasIndex(reception => reception.Status)
            .HasDatabaseName("IX_DocumentaryReceptions_Status");
        builder.HasIndex(reception => reception.ReceivedAtUtc)
            .HasDatabaseName("IX_DocumentaryReceptions_ReceivedAtUtc");
    }
}
