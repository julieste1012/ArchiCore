using ArchiCore.Domain.Loans;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiCore.Infrastructure.Persistence.Configurations;

internal sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        builder.Ignore(x => x.DomainEvents);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.FolderId).IsRequired();
        builder.Property(x => x.RequestedByUserId).IsRequired();
        builder.Property(x => x.RequestedAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.DueAtUtc).HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.AuthorizedByUserId);
        builder.Property(x => x.AuthorizedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.DeliveredByUserId);
        builder.Property(x => x.DeliveredAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.ReturnedByUserId);
        builder.Property(x => x.ReturnedAtUtc).HasColumnType("timestamp with time zone");
        builder.Property(x => x.OverdueDetectedAtUtc).HasColumnType("timestamp with time zone");
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.FolderId);
        builder.HasIndex(x => new { x.Status, x.DueAtUtc });
    }
}
